// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Meshy
{
    [Preserve]
    internal sealed class TaskResponse
    {
        [Preserve]
        [JsonConstructor]
        public TaskResponse([JsonProperty("result")] string result)
        {
            Result = result;
        }

        [Preserve]
        [JsonProperty("result")]
        public string Result { get; }
    }
}
