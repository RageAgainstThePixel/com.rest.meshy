// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meshy.TextToTexture
{
    /// <summary>
    /// Quickly generate high-quality textures for your existing 3D models using text prompts and concept art.
    /// </summary>
    public sealed class TextToTextureEndpoint : MeshyBaseEndpoint
    {
        public TextToTextureEndpoint(MeshyClient client) : base(client) { }

        protected override string Root => "text-to-texture";

        /// <summary>
        /// This endpoint allows you to create a new Text to Texture task.
        /// </summary>
        /// <returns>Job id.</returns>
        public async Task CreateTextToTextureTaskAsync(TextToTextureRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// This endpoint allows you to retrieve a Text to Texture task given a valid task id.
        /// </summary>
        /// <returns><see cref="MeshyTaskResult"/></returns>
        public async Task RetrieveTextToTextureTaskAsync(string id, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
