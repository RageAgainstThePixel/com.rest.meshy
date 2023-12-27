// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Meshy.Account
{
    [Preserve]
    public sealed class SubscriptionInfo
    {
        [Preserve]
        [JsonConstructor]
        public SubscriptionInfo(
            [JsonProperty("tier")] string tier,
            [JsonProperty("refillAt")] long refillAtUnixTimeMilliseconds,
            [JsonProperty("renewAt")] long renewAtUnixTimeMilliseconds,
            [JsonProperty("cancelAt")] long cancelAtUnitTimeMilliseconds)
        {
            Tier = tier;
            RefillAtUnixTimeMilliseconds = refillAtUnixTimeMilliseconds;
            RenewAtUnixTimeMilliseconds = renewAtUnixTimeMilliseconds;
            CancelAtUnitTimeMilliseconds = cancelAtUnitTimeMilliseconds;
        }

        [Preserve]
        [JsonProperty("tier")]
        public string Tier { get; }

        [Preserve]
        [JsonProperty("refillAt")]
        public long RefillAtUnixTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime RefillAt => DateTimeOffset.FromUnixTimeMilliseconds(RefillAtUnixTimeMilliseconds).DateTime;

        [Preserve]
        [JsonProperty("renewAt")]
        public long RenewAtUnixTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime RenewAt => DateTimeOffset.FromUnixTimeMilliseconds(RenewAtUnixTimeMilliseconds).DateTime;

        [Preserve]
        [JsonProperty("cancelAt")]
        public long CancelAtUnitTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime CancelAt => DateTimeOffset.FromUnixTimeMilliseconds(CancelAtUnitTimeMilliseconds).DateTime;
    }
}
