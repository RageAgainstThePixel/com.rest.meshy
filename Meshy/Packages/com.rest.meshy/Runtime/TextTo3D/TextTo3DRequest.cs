// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Meshy.TextTo3D
{
    [Preserve]
    public sealed class TextTo3DRequest
    {
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
        public string ObjectPrompt { get; }

        /// <summary>
        /// Describe your desired style of the object.
        /// </summary>
        [Preserve]
        [JsonProperty("style_prompt")]
        public string StylePrompt { get; }

        /// <summary>
        /// Generate PBR Maps (metallic, roughness, normal) in addition to the base color.
        /// Default to true if not specified.
        /// </summary>
        [Preserve]
        [JsonProperty("enable_pbr")]
        public bool? EnablePBR { get; }

        /// <summary>
        /// Describe what the model should not look like.
        /// </summary>
        [Preserve]
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; }

        /// <summary>
        /// Specify the resolution of generated textures.
        /// Available values: 1024, 2048, 4096
        /// </summary>
        [Preserve]
        [JsonProperty("resolution")]
        public string Resolution { get; }

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
        public string ArtStyle { get; }
    }
}
