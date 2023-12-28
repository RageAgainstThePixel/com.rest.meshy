// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_02_TextTo3D : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetTextTo3DTasks()
        {
            Assert.IsNotNull(MeshyClient.TextTo3DEndpoint);
            var textTo3DTasks = await MeshyClient.TextTo3DEndpoint.ListTasksAsync(1, 12, SortOrder.Ascending);
            Assert.IsNotNull(textTo3DTasks);

            foreach (var meshyTask in textTo3DTasks)
            {
                Debug.Log($"{meshyTask.Id} | created_at: {meshyTask.CreatedAt}");
            }
        }
    }
}
