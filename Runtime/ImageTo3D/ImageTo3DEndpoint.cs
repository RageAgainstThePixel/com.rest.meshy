// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Utilities.WebRequestRest;

namespace Meshy.ImageTo3D
{
    /// <summary>
    /// Quickly transform your 2D images into stunning 3D models and bring your visuals to life.
    /// </summary>
    public class ImageTo3DEndpoint : MeshyBaseTaskEndpoint
    {
        public ImageTo3DEndpoint(MeshyClient client) : base(client) { }

        protected override string Root => "image-to-3d";

        /// <summary>
        /// Create image to 3d task.
        /// </summary>
        /// <param name="request"><see cref="ImageTo3DRequest"/>.</param>
        /// <param name="progress">Optional, <see cref="IProgress{TaskProgress}"/> callback.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="MeshyTaskResult"/>.</returns>
        public async Task<MeshyTaskResult> CreateImageTo3DTaskAsync(ImageTo3DRequest request, IProgress<TaskProgress> progress = null, CancellationToken cancellationToken = default)
        {
            Response response;

            if (request.Image == null)
            {
                var payload = JsonConvert.SerializeObject(request, MeshyClient.JsonSerializationOptions);
                response = await Rest.PostAsync(GetUrl(), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            }
            else
            {
                var form = new WWWForm();
                form.AddBinaryData("image_file", request.Image.EncodeToPNG(), $"{UnityWebRequest.EscapeURL(request.Image.name)}.png", "multipart/form-data");

                if (request.EnablePBR.HasValue)
                {
                    form.AddField("enable_pbr", request.EnablePBR.Value.ToString());
                }

                response = await Rest.PostAsync(GetUrl(), form, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            }

            response.Validate(EnableDebug);
            var taskId = JsonConvert.DeserializeObject<TaskResponse>(response.Body, MeshyClient.JsonSerializationOptions)?.Result;

            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new Exception($"Failed to get a valid {nameof(taskId)}! \n{response.Body}");
            }

            return await PollTaskProgressAsync(taskId, progress, cancellationToken);
        }
    }
}
