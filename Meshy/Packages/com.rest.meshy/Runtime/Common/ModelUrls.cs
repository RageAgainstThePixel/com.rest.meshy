// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Meshy
{
    [Preserve]
    public sealed class ModelUrls
    {
        [Preserve]
        [JsonConstructor]
        public ModelUrls(
            [JsonProperty("glb")] string glb,
            [JsonProperty("fbx")] string fbx,
            [JsonProperty("usdz")] string usdz)
        {
            Glb = glb;
            Fbx = fbx;
            Usdz = usdz;
        }

        /// <summary>
        /// Downloadable URL to the GLB file.
        /// </summary>
        [Preserve]
        [JsonProperty("glb")]
        public string Glb { get; }

        /// <summary>
        /// Downloadable URL to the FBX file.
        /// </summary>
        [Preserve]
        [JsonProperty("fbx")]
        public string Fbx { get; }

        /// <summary>
        /// Downloadable URL to the USDZ file.
        /// </summary>
        [Preserve]
        [JsonProperty("usdz")]
        public string Usdz { get; }
    }
}
