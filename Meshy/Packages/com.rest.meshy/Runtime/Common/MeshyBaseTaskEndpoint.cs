// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
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
        /// List all of the tasks for this endpoint.
        /// </summary>
        /// <param name="pageNumber">Optional, page number.</param>
        /// <param name="pageSize">Optional, number of items per page.</param>
        /// <param name="order">Optional, <see cref="SortOrder"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IReadOnlyList{MeshyTaskResult}"/>.</returns>
        public async Task<IReadOnlyList<MeshyTaskResult>> ListTasksAsync(int? pageNumber = null, int? pageSize = null, SortOrder order = SortOrder.Descending, CancellationToken cancellationToken = default)
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

            switch (order)
            {
                case SortOrder.Ascending:
                    query.Add("sortBy", "+created_at");
                    break;
                case SortOrder.Descending:
                    query.Add("sortBy", "-created_at");
                    break;
            }

            var response = await Rest.GetAsync(GetUrl(queryParameters: query), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<MeshyTaskResult>>(response.Body, MeshyClient.JsonSerializationOptions);
        }

        /// <summary>
        /// This endpoint allows you to retrieve a task by its id.
        /// </summary>
        /// <param name="id">Id of the task.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="MeshyTaskResult"/></returns>
        public async Task<MeshyTaskResult> RetrieveTextToTextureTaskAsync(string id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<MeshyTaskResult>(response.Body, MeshyClient.JsonSerializationOptions);
        }
    }
}
