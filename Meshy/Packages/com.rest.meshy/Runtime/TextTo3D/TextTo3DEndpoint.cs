// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace Meshy.TextTo3D
{
    /// <summary>
    /// Quickly generate high-quality 3d models using text prompts.
    /// </summary>
    public class TextTo3DEndpoint : MeshyBaseTaskEndpoint
    {
        public TextTo3DEndpoint(MeshyClient client) : base(client) { }

        protected override string Root => "text-to-3d";

        /// <summary>
        /// Creates a new text to 3d Task.
        /// </summary>
        /// <param name="request"><see cref="TextTo3DRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Task Id.</returns>
        public async Task<string> CreateTextTo3DTaskAsync(TextTo3DRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(request, MeshyClient.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl(), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<TaskResponse>(response.Body, MeshyClient.JsonSerializationOptions)?.Result;
        }
    }
}
