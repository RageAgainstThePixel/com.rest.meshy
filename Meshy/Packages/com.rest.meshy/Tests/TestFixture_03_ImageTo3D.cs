// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.ImageTo3D;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_03_ImageTo3D : AbstractTestFixture
    {
        private readonly string testImageUrl = "";

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

        [Test]
        public async Task Test_02_01_CreateImageTo3DTask_URL()
        {
            Assert.IsNotNull(MeshyClient.ImageTo3DEndpoint);
            var request = new ImageTo3DRequest(testImageUrl);
            var taskResult = await MeshyClient.ImageTo3DEndpoint.CreateImageTo3DTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
        }
    }
}
