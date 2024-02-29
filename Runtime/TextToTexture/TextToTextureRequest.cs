// Licensed under the MIT License. See LICENSE in the project root for license information.

using GLTFast.Export;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Meshy.TextToTexture
{
    [Preserve]
    public sealed class TextToTextureRequest : IMeshyTaskRequest
    {
        [Preserve]
        public TextToTextureRequest(
            GameObject model,
            string objectPrompt,
            string stylePrompt,
            bool? enableOriginalUV = true,
            bool? enablePBR = true,
            string negativePrompt = null,
            string resolution = null,
            string artStyle = null)
            : this(string.Empty,
                objectPrompt,
                stylePrompt,
                enableOriginalUV,
                enablePBR,
                negativePrompt,
                resolution,
                artStyle)
        {
            Model = model;
        }

        [Preserve]
        public TextToTextureRequest(
            GameObjectExport glbExport,
            string objectPrompt,
            string stylePrompt,
            bool? enableOriginalUV = true,
            bool? enablePBR = true,
            string negativePrompt = null,
            string resolution = null,
            string artStyle = null)
            : this(string.Empty,
                objectPrompt,
                stylePrompt,
                enableOriginalUV,
                enablePBR,
                negativePrompt,
                resolution,
                artStyle)
        {
            GlbExport = glbExport;
        }

        [Preserve]
        [JsonConstructor]
        public TextToTextureRequest(
            [JsonProperty("model_url")] string modelUrl,
            [JsonProperty("object_prompt")] string objectPrompt,
            [JsonProperty("style_prompt")] string stylePrompt,
            [JsonProperty("enable_original_uv")] bool? enableOriginalUV = true,
            [JsonProperty("enable_pbr")] bool? enablePBR = true,
            [JsonProperty("negative_prompt")] string negativePrompt = null,
            [JsonProperty("resolution")] string resolution = null,
            [JsonProperty("art_style")] string artStyle = null)
        {
            ModelUrl = modelUrl;
            ObjectPrompt = objectPrompt;
            StylePrompt = stylePrompt;
            EnableOriginalUV = enableOriginalUV;
            EnablePBR = enablePBR;
            NegativePrompt = negativePrompt;
            Resolution = resolution;
            ArtStyle = artStyle;
        }

        [Preserve]
        [JsonIgnore]
        public GameObject Model { get; internal set; }

        [Preserve]
        [JsonIgnore]
        public GameObjectExport GlbExport { get; internal set; }

        /// <summary>
        /// Downloadable URL to the 3D model for Meshy to texture.
        /// Currently, we support .fbx / .obj / .stl / .gltf / .glb models.
        /// Please also check the 'Model Requirements' subsection in 'Upload Your Model' for any additional requirements.
        /// </summary>
        /// <remarks>
        /// Meshy will validate and only accept 3D models in supported formats,
        /// passing a unsupported format with model_url will result in a 400 error.
        /// </remarks>
        [Preserve]
        [JsonProperty("model_url")]
        public string ModelUrl { get; internal set; }

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
        /// Use the original UV of the model instead of generating new UVs.
        /// If the model has no original UV, the quality of the output might not be as good.
        /// Default to true if not specified.
        /// </summary>
        [JsonProperty("enable_original_uv")]
        public bool? EnableOriginalUV { get; internal set; }

        /// <summary>
        /// Generate PBR Maps (metallic, roughness, normal) in addition to the base color.
        /// Default to true if not specified.
        /// </summary>
        [Preserve]
        [JsonProperty("enable_pbr")]
        public bool? EnablePBR { get; internal set; }

        /// <summary>
        /// Optional, Describe what the texture should not look like.
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
        /// Available values: realistic, fake-3d-cartoon,
        /// japanese-anime, cartoon-line-art, realistic-hand-drawn,
        /// fake-3d-hand-drawn, oriental-comic-ink
        /// </summary>
        [Preserve]
        [JsonProperty("art_style")]
        public string ArtStyle { get; internal set; }
    }
}
