// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_01_Account : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_SubscriptionInfo()
        {
            Assert.IsNotNull(MeshyClient.AccountEndpoint);
            var subscriptionInfo = await MeshyClient.AccountEndpoint.GetSubscriptionInfoAsync();
            Assert.IsNotNull(subscriptionInfo);
            Debug.Log($"Tier: {subscriptionInfo.Tier} | Balance Refills at: {subscriptionInfo.RefillAt}");
        }

        [Test]
        public async Task Test_02_CreditBalance()
        {
            Assert.IsNotNull(MeshyClient.AccountEndpoint);
            var subscriptionInfo = await MeshyClient.AccountEndpoint.GetCreditBalanceAsync();
            Assert.IsNotNull(subscriptionInfo);
            Debug.Log($"Credits: {subscriptionInfo.CreditBalance} | Free Credits: {subscriptionInfo.FreeCreditBalance} | Earned Credits: {subscriptionInfo.ShareCreditEarned} | Credits Earned Today: {subscriptionInfo.ShareCreditEarnedToday}");
        }
    }
}
