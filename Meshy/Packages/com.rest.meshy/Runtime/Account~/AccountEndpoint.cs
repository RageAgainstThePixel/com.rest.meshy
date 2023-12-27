// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace Meshy.Account
{
    /// <summary>
    /// Get information about your meshy account.
    /// </summary>
    public sealed class AccountEndpoint : MeshyBaseEndpoint
    {
        public AccountEndpoint(MeshyClient client) : base(client) { }

        protected override string Root => "me";

        /// <summary>
        /// Get your account credit balance.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="CreditBalanceInfo"/></returns>
        public async Task<CreditBalanceInfo> GetCreditBalanceAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("/credits"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<CreditBalanceInfo>(response.Body, MeshyClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Get your account subscription info.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SubscriptionInfo"/></returns>
        public async Task<SubscriptionInfo> GetSubscriptionInfoAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("/tier"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<SubscriptionInfo>(response.Body, MeshyClient.JsonSerializationOptions);
        }
    }
}
