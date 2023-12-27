// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Meshy
{
    [Preserve]
    public sealed class TextureUrls
    {
        [Preserve]
        [JsonConstructor]
        public TextureUrls(
            [JsonProperty("base_color")] string baseColor,
            [JsonProperty("metallic")] string metallic,
            [JsonProperty("normal")] string normal,
            [JsonProperty("roughness")] string roughness)
        {
        }

        /// <summary>
        /// Downloadable URL to the base color map image.
        /// </summary>
        [Preserve]
        [JsonProperty("base_color")]
        public string BaseColor { get; }

        /// <summary>
        /// Downloadable URL to the metallic map image.
        /// </summary>
        /// <remarks>
        /// If the task is created with enable_pbr: false, this property will be omitted.
        /// </remarks>
        [Preserve]
        [JsonProperty("metallic")]
        public string Metallic { get; }

        /// <summary>
        /// Downloadable URL to the normal map image.
        /// </summary>
        /// <remarks>
        /// If the task is created with enable_pbr: false, this property will be omitted.
        /// </remarks>
        [Preserve]
        [JsonProperty("normal")]
        public string Normal { get; }

        /// <summary>
        /// Downloadable URL to the roughness map image.
        /// </summary>
        /// <remarks>
        /// If the task is created with enable_pbr: false, this property will be omitted.
        /// </remarks>
        [Preserve]
        [JsonProperty("roughness")]
        public string Roughness { get; }
    }
}
