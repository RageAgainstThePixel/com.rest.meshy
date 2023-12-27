// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_01_TextToTexture : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetTextToTextureTasks()
        {
            Assert.IsNotNull(MeshyClient.TextToTextureEndpoint);
            var textToTextureTasks = await MeshyClient.TextToTextureEndpoint.ListTasksAsync(1, 12, SortOrder.Ascending);
            Assert.IsNotNull(textToTextureTasks);

            foreach (var meshyTask in textToTextureTasks)
            {
                Debug.Log($"{meshyTask.Id} | created_at: {meshyTask.CreatedAt}");
            }
        }
    }
}
