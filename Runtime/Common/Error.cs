// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Meshy
{
    [Preserve]
    public sealed class Error
    {
        [Preserve]
        [JsonConstructor]
        public Error([JsonProperty("message")] string message)
        {
            Message = message;
        }

        /// <summary>
        /// Detailed error message.
        /// </summary>
        [Preserve]
        [JsonProperty("message")]
        public string Message { get; }
    }
}
