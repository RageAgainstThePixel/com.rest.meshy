// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.TextTo3D;
using NUnit.Framework;
using System;
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

        [Test]
        [Timeout(720000)]
        public async Task Test_02_CreateTextTo3DTask()
        {
            Assert.IsNotNull(MeshyClient.TextTo3DEndpoint);
            var request = new TextTo3DRequest("Lantern", "game assets", resolution: Resolutions.X1024, artStyle: ArtStyles.Realistic);
            var taskResult = await MeshyClient.TextTo3DEndpoint.CreateTextTo3DTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }
    }
}
