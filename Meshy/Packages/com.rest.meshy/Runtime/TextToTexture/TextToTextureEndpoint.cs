// Licensed under the MIT License. See LICENSE in the project root for license information.

using GLTFast;
using GLTFast.Export;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace Meshy.TextToTexture
{
    /// <summary>
    /// Quickly generate high-quality textures for your existing 3D models using text prompts and concept art.
    /// </summary>
    public sealed class TextToTextureEndpoint : MeshyBaseTaskEndpoint
    {
        public TextToTextureEndpoint(MeshyClient client) : base(client) { }

        protected override string Root => "text-to-texture";

        /// <summary>
        /// Creates a new text to texture task.
        /// </summary>
        /// <param name="request"><see cref="TextToTextureRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Task Id.</returns>
        public async Task<string> CreateTextToTextureTaskAsync(TextToTextureRequest request, CancellationToken cancellationToken = default)
        {
            Response response;

            if (request.Model != null || request.GlbExport != null)
            {
                if (request.GlbExport == null)
                {
                    var exportSettings = new ExportSettings
                    {
                        Format = GltfFormat.Binary,
                        FileConflictResolution = FileConflictResolution.Overwrite,
                        ComponentMask = ~(ComponentType.Camera | ComponentType.Animation | ComponentType.Light),
                    };
                    var gameObjectExportSettings = new GameObjectExportSettings
                    {
                        OnlyActiveInHierarchy = false,
                        DisabledComponents = true,
                    };
                    request.GlbExport = new GameObjectExport(exportSettings, gameObjectExportSettings);
                    var position = request.Model.transform.position;
                    var rotation = request.Model.transform.rotation;
                    // reset object pose for export.
                    request.Model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    request.GlbExport.AddScene(new[] { request.Model });
                    // put object back where it belongs after export.
                    request.Model.transform.SetPositionAndRotation(position, rotation);
                }

                var form = new WWWForm();
                using var glbStream = new MemoryStream();

                if (!await request.GlbExport.SaveToStreamAndDispose(glbStream, cancellationToken))
                {
                    throw new Exception($"Failed to export {request.Model.name} to .glb!");
                }

                form.AddBinaryData("model_file", glbStream.ToArray(), $"{request.Model.name}.glb", "model/gltf-binary");
                form.AddField("object_prompt", request.ObjectPrompt);
                form.AddField("style_prompt", request.StylePrompt);

                if (request.EnableOriginalUV.HasValue)
                {
                    form.AddField("enable_original_uv", request.EnableOriginalUV.Value.ToString());
                }

                if (request.EnablePBR.HasValue)
                {
                    form.AddField("enable_pbr", request.EnablePBR.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(request.NegativePrompt))
                {
                    form.AddField("negative_prompt", request.NegativePrompt);
                }

                if (!string.IsNullOrWhiteSpace(request.Resolution))
                {
                    form.AddField("resolution", request.Resolution);
                }

                if (!string.IsNullOrWhiteSpace(request.ArtStyle))
                {
                    form.AddField("art_style", request.ArtStyle);
                }

                response = await Rest.PostAsync(GetUrl(), form, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            }
            else
            {
                var payload = JsonConvert.SerializeObject(request, MeshyClient.JsonSerializationOptions);
                response = await Rest.PostAsync(GetUrl(), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            }

            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<TaskResponse>(response.Body, MeshyClient.JsonSerializationOptions)?.Result;
        }
    }
}
