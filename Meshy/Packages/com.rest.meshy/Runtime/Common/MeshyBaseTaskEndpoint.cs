// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace Meshy
{
    public abstract class MeshyBaseTaskEndpoint : MeshyBaseEndpoint
    {
        protected MeshyBaseTaskEndpoint(MeshyClient client) : base(client) { }

        /// <summary>
        /// The polling interval to check pending or in progress task statuses.
        /// </summary>
        public int PollingIntervalMilliseconds { get; set; } = 500;

        /// <summary>
        /// List all of the tasks for this endpoint.
        /// </summary>
        /// <param name="pageNumber">Optional, page number.</param>
        /// <param name="pageSize">Optional, number of items per page.</param>
        /// <param name="order">Optional, <see cref="SortOrder"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IReadOnlyList{MeshyTaskResult}"/>.</returns>
        public async Task<IReadOnlyList<MeshyTaskResult>> ListTasksAsync(int? pageNumber = null, int? pageSize = null, SortOrder? order = null, CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string>();

            if (pageNumber.HasValue)
            {
                query.Add("pageNum", pageNumber.Value.ToString());
            }

            if (pageSize.HasValue)
            {
                query.Add("pageSize", pageSize.Value.ToString());
            }

            if (order.HasValue)
            {
                switch (order)
                {
                    case SortOrder.Ascending:
                        query.Add("sortBy", "+created_at");
                        break;
                    case SortOrder.Descending:
                        query.Add("sortBy", "-created_at");
                        break;
                }
            }

            var response = await Rest.GetAsync(GetUrl(queryParameters: query), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<MeshyTaskResult>>(response.Body, MeshyClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Retrieve a task by its id.
        /// </summary>
        /// <param name="taskId">Id of the task.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="MeshyTaskResult"/></returns>
        public async Task<MeshyTaskResult> RetrieveTaskAsync(string taskId, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"/{taskId}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<MeshyTaskResult>(response.Body, MeshyClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Polls the task progress until it succeeds, fails, or expires.
        /// </summary>
        /// <param name="taskId">Task id.</param>
        /// <param name="progress">Optional, <see cref="IProgress{TaskProgress}"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="MeshyTaskResult"/>.</returns>
        internal async Task<MeshyTaskResult> PollTaskProgressAsync(string taskId, IProgress<TaskProgress> progress = null, CancellationToken cancellationToken = default)
        {
            var taskResult = await RetrieveTaskAsync(taskId, cancellationToken);

            if (taskResult == null)
            {
                throw new Exception($"Failed to get a valid {nameof(taskResult)} for task \"{taskId}\"!");
            }

            progress?.Report(taskResult);

            switch (taskResult.Status)
            {
                case Status.Pending:
                case Status.InProgress:
                    await Task.Delay(PollingIntervalMilliseconds, cancellationToken).ConfigureAwait(true);
                    return await PollTaskProgressAsync(taskId, progress, cancellationToken);
                case Status.Succeeded:
                case Status.Failed:
                case Status.Expired:
                    return taskResult;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
