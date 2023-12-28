// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Meshy.ImageTo3D
{
    [Preserve]
    public sealed class ImageTo3DRequest
    {
        [Preserve]
        public ImageTo3DRequest(Texture2D image, bool? enablePBR = true)
        {
            Image = image;
            EnablePBR = enablePBR;
        }

        [Preserve]
        [JsonConstructor]
        public ImageTo3DRequest(
            [JsonProperty("image_url")] string imageUrl,
            [JsonProperty("enable_pbr")] bool? enablePBR = true)
        {
            ImageUrl = imageUrl;
            EnablePBR = enablePBR;
        }

        [Preserve]
        [JsonIgnore]
        public Texture2D Image { get; internal set; }

        [Preserve]
        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        [Preserve]
        [JsonProperty("enable_pbr")]
        public bool? EnablePBR { get; internal set; }
    }
}
