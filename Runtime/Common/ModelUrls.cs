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
            [JsonProperty("usdz")] string usdz,
            [JsonProperty("obj")] string obj,
            [JsonProperty("mtl")] string mtl)
        {
            Glb = glb;
            Fbx = fbx;
            Usdz = usdz;
            Obj = obj;
            Mtl = mtl;
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

        /// <summary>
        /// Downloadable URL to the Obj file.
        /// </summary>
        [Preserve]
        [JsonProperty("obj")]
        public string Obj { get; }

        /// <summary>
        /// Downloadable URL to the Mtl file.
        /// </summary>
        [Preserve]
        [JsonProperty("mtl")]
        public string Mtl { get; }
    }
}
