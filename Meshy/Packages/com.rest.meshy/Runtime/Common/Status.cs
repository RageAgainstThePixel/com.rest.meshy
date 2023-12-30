// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Meshy
{
    [Preserve]
    public enum Status
    {
        [EnumMember(Value = "PENDING")]
        Pending,
        [EnumMember(Value = "IN_PROGRESS")]
        InProgress,
        [EnumMember(Value = "SUCCEEDED")]
        Succeeded,
        [EnumMember(Value = "FAILED")]
        Failed,
        [EnumMember(Value = "EXPIRED")]
        Expired
    }
}
