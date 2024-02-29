// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.TextTo3D;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
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

        internal string GetEndpointWithVersion<T>(string endpoint = "", Dictionary<string, string> queryParameters = null)
            where T : IMeshyTaskRequest
        {
            string version;

            if (typeof(T).IsAssignableFrom(typeof(TextTo3DBetaPreviewRequest)) ||
                typeof(T).IsAssignableFrom(typeof(TextTo3DBetaRefineRequest)))
            {
                version = "v2";
            }
            else
            {
                version = "v1";
            }

            return GetUrl($"{version}/{Root}{endpoint}", queryParameters);
        }

        protected override string GetUrl(string endpoint = "", Dictionary<string, string> queryParameters = null)
        {
            var result = string.Format(client.Settings.BaseRequestUrlFormat, endpoint);

            if (queryParameters is { Count: not 0 })
            {
                result += $"?{string.Join("&", queryParameters.Select(parameter => $"{UnityWebRequest.EscapeURL(parameter.Key)}={UnityWebRequest.EscapeURL(parameter.Value)}"))}";
            }

            return result;
        }

        /// <summary>
        /// List all the tasks for this endpoint by task type.
        /// </summary>
        /// <typeparam name="T">The <see cref="IMeshyTaskRequest"/> to filter by.</typeparam>
        /// <param name="pageNumber">Optional, page number.</param>
        /// <param name="pageSize">Optional, number of items per page.</param>
        /// <param name="order">Optional, <see cref="SortOrder"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IReadOnlyList{MeshyTaskResult}"/>.</returns>
        public async Task<IReadOnlyList<MeshyTaskResult>> ListTasksAsync<T>(int? pageNumber = null, int? pageSize = null, SortOrder? order = null, CancellationToken cancellationToken = default)
            where T : IMeshyTaskRequest
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

            var response = await Rest.GetAsync(GetEndpointWithVersion<T>(queryParameters: query), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<MeshyTaskResult>>(response.Body, MeshyClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Retrieve a task by its id.
        /// </summary>
        /// <param name="taskId">The id of the task.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="MeshyTaskResult"/></returns>
        public async Task<MeshyTaskResult> RetrieveTaskAsync<T>(string taskId, CancellationToken cancellationToken = default)
            where T : IMeshyTaskRequest
        {
            var response = await Rest.GetAsync(GetEndpointWithVersion<T>($"/{taskId}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
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
        internal async Task<MeshyTaskResult> PollTaskProgressAsync<T>(string taskId, IProgress<TaskProgress> progress = null, CancellationToken cancellationToken = default)
            where T : IMeshyTaskRequest
        {
            try
            {
                var taskResult = await RetrieveTaskAsync<T>(taskId, cancellationToken);

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
                        return await PollTaskProgressAsync<T>(taskId, progress, cancellationToken);
                    case Status.Succeeded:
                    case Status.Failed:
                    case Status.Expired:
                        return taskResult;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (RestException restEx)
            {
                if (restEx.Response.Code == 429)
                {
                    await Task.Delay(PollingIntervalMilliseconds * 2, cancellationToken).ConfigureAwait(true);
                    return await PollTaskProgressAsync<T>(taskId, progress, cancellationToken);
                }

                throw;
            }
        }
    }
}
