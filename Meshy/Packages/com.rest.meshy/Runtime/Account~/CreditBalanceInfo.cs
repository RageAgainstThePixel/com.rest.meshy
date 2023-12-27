// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Meshy.Account
{
    [Preserve]
    public sealed class CreditBalanceInfo
    {
        [Preserve]
        [JsonConstructor]
        public CreditBalanceInfo(
            int creditBalance,
            int freeCreditBalance,
            int shareCreditEarned,
            int shareCreditEarnedToday)
        {
            CreditBalance = creditBalance;
            FreeCreditBalance = freeCreditBalance;
            ShareCreditEarned = shareCreditEarned;
            ShareCreditEarnedToday = shareCreditEarnedToday;
        }

        [Preserve]
        [JsonProperty("creditBalance")]
        public int CreditBalance { get; }

        [Preserve]
        [JsonProperty("freeCreditBalance")]
        public int FreeCreditBalance { get; }

        [Preserve]
        [JsonProperty("shareCreditEarned")]
        public int ShareCreditEarned { get; }

        [Preserve]
        [JsonProperty("shareCreditEarnedToday")]
        public int ShareCreditEarnedToday { get; }
    }
}
