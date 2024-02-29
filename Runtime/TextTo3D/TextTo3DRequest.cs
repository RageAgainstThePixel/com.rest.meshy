// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Meshy.TextTo3D
{
    public interface IMeshyTextTo3DRequest : IMeshyTaskRequest { }

    [Preserve]
    [Obsolete("Use TextTo3DAlphaRequest instead.")]
    public sealed class TextTo3DRequest : IMeshyTextTo3DRequest
    {
        public static implicit operator TextTo3DAlphaRequest(TextTo3DRequest request)
            => new(request.ObjectPrompt, request.StylePrompt, request.EnablePBR, request.NegativePrompt, request.Resolution, request.ArtStyle);

        [Preserve]
        [JsonConstructor]
        public TextTo3DRequest(
            [JsonProperty("object_prompt")] string objectPrompt,
            [JsonProperty("style_prompt")] string stylePrompt,
            [JsonProperty("enable_pbr")] bool? enablePBR = true,
            [JsonProperty("negative_prompt")] string negativePrompt = null,
            [JsonProperty("resolution")] string resolution = null,
            [JsonProperty("art_style")] string artStyle = null)
        {
            ObjectPrompt = objectPrompt;
            StylePrompt = stylePrompt;
            EnablePBR = enablePBR;
            NegativePrompt = negativePrompt;
            Resolution = resolution;
            ArtStyle = artStyle;
        }

        /// <summary>
        /// Describe what kind of object the 3D model is.
        /// </summary>
        [Preserve]
        [JsonProperty("object_prompt")]
        public string ObjectPrompt { get; internal set; }

        /// <summary>
        /// Describe your desired style of the object.
        /// </summary>
        [Preserve]
        [JsonProperty("style_prompt")]
        public string StylePrompt { get; internal set; }

        /// <summary>
        /// Generate PBR Maps (metallic, roughness, normal) in addition to the base color.
        /// Default to true if not specified.
        /// </summary>
        [Preserve]
        [JsonProperty("enable_pbr")]
        public bool? EnablePBR { get; internal set; }

        /// <summary>
        /// Describe what the model should not look like.
        /// </summary>
        [Preserve]
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; internal set; }

        /// <summary>
        /// Specify the resolution of generated textures.
        /// Available values: 1024, 2048, 4096
        /// </summary>
        [Preserve]
        [JsonProperty("resolution")]
        public string Resolution { get; internal set; }

        /// <summary>
        /// Describe your desired art style of the object.
        /// Default to realistic if not specified.
        /// Available values: realistic, voxel, fake-3d-cartoon,
        /// japanese-anime, cartoon-line-art, realistic-hand-drawn,
        /// fake-3d-hand-drawn, oriental-comic-ink.
        /// </summary>
        /// <remarks>
        /// If art_style is set to voxel, enable_pbr will always be false regardless of the provided value.
        /// </remarks>
        [Preserve]
        [JsonProperty("art_style")]
        public string ArtStyle { get; internal set; }
    }

    [Preserve]
    public sealed class TextTo3DAlphaRequest : IMeshyTextTo3DRequest
    {
        [Preserve]
        [JsonConstructor]
        public TextTo3DAlphaRequest(
            [JsonProperty("object_prompt")] string objectPrompt,
            [JsonProperty("style_prompt")] string stylePrompt,
            [JsonProperty("enable_pbr")] bool? enablePBR = true,
            [JsonProperty("negative_prompt")] string negativePrompt = null,
            [JsonProperty("resolution")] string resolution = null,
            [JsonProperty("art_style")] string artStyle = null)
        {
            ObjectPrompt = objectPrompt;
            StylePrompt = stylePrompt;
            EnablePBR = enablePBR;
            NegativePrompt = negativePrompt;
            Resolution = resolution;
            ArtStyle = artStyle;
        }

        /// <summary>
        /// Describe what kind of object the 3D model is.
        /// </summary>
        [Preserve]
        [JsonProperty("object_prompt")]
        public string ObjectPrompt { get; internal set; }

        /// <summary>
        /// Describe your desired style of the object.
        /// </summary>
        [Preserve]
        [JsonProperty("style_prompt")]
        public string StylePrompt { get; internal set; }

        /// <summary>
        /// Generate PBR Maps (metallic, roughness, normal) in addition to the base color.
        /// Default to true if not specified.
        /// </summary>
        [Preserve]
        [JsonProperty("enable_pbr")]
        public bool? EnablePBR { get; internal set; }

        /// <summary>
        /// Describe what the model should not look like.
        /// </summary>
        [Preserve]
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; internal set; }

        /// <summary>
        /// Specify the resolution of generated textures.
        /// Available values: 1024, 2048, 4096
        /// </summary>
        [Preserve]
        [JsonProperty("resolution")]
        public string Resolution { get; internal set; }

        /// <summary>
        /// Describe your desired art style of the object.
        /// Default to realistic if not specified.
        /// Available values: realistic, voxel, fake-3d-cartoon,
        /// japanese-anime, cartoon-line-art, realistic-hand-drawn,
        /// fake-3d-hand-drawn, oriental-comic-ink.
        /// </summary>
        /// <remarks>
        /// If art_style is set to voxel, enable_pbr will always be false regardless of the provided value.
        /// </remarks>
        [Preserve]
        [JsonProperty("art_style")]
        public string ArtStyle { get; internal set; }
    }

    [Preserve]
    public sealed class TextTo3DBetaPreviewRequest : IMeshyTextTo3DRequest
    {
        [Preserve]
        public TextTo3DBetaPreviewRequest(string prompt, string negativePrompt = null, string artStyle = null, int? seed = null)
        {
            Prompt = prompt;
            NegativePrompt = negativePrompt;
            ArtStyle = artStyle;
            Seed = seed;
        }

        [Preserve]
        [JsonProperty("mode")]
        public string Mode => "preview";

        [Preserve]
        [JsonProperty("prompt")]
        public string Prompt { get; internal set; }

        [Preserve]
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; internal set; }

        [Preserve]
        [JsonProperty("art_style")]
        public string ArtStyle { get; internal set; }

        [Preserve]
        [JsonProperty("seed")]
        public int? Seed { get; internal set; }
    }

    [Preserve]
    public sealed class TextTo3DBetaRefineRequest : IMeshyTextTo3DRequest
    {
        [Preserve]
        public TextTo3DBetaRefineRequest(MeshyTaskResult previewTask, TextureRichness textureRichness = TextureRichness.Medium)
        {
            PreviewTaskId = previewTask.Id;
            TextureRichness = textureRichness;
        }

        [Preserve]
        [JsonProperty("mode")]
        public string Mode => "refine";

        [Preserve]
        [JsonProperty("preview_task_id")]
        public string PreviewTaskId { get; }

        [Preserve]
        [JsonProperty("texture_richness")]
        public TextureRichness TextureRichness { get; internal set; }
    }
}
