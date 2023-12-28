// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_03_ImageTo3D : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetImageTo3DTasks()
        {
            Assert.IsNotNull(MeshyClient.ImageTo3DEndpoint);
            var imageTo3dTasks = await MeshyClient.ImageTo3DEndpoint.ListTasksAsync(1, 12, SortOrder.Ascending);
            Assert.IsNotNull(imageTo3dTasks);

            foreach (var meshyTask in imageTo3dTasks)
            {
                Debug.Log($"{meshyTask.Id} | created_at: {meshyTask.CreatedAt}");
            }
        }
    }
}
