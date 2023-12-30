// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Meshy
{
    [Preserve]
    public enum SortType
    {
        [EnumMember(Value = "created_at")]
        CreatedAt,
        [EnumMember(Value = "updated_at")]
        UpdatedAt,
    }
}
