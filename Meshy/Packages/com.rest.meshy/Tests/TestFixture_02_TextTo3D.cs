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
        public async Task Test_01_GetTextTo3DTasks_Alpha()
        {
            Assert.IsNotNull(MeshyClient.TextTo3DEndpoint);
            var textTo3DTasks = await MeshyClient.TextTo3DEndpoint.ListTasksAsync<TextTo3DAlphaRequest>(1, 12, SortOrder.Ascending);
            Assert.IsNotNull(textTo3DTasks);

            foreach (var meshyTask in textTo3DTasks)
            {
                Debug.Log($"{meshyTask.Id} | created_at: {meshyTask.CreatedAt}");
            }
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_CreateTextTo3DTask_Alpha()
        {
            Assert.IsNotNull(MeshyClient.TextTo3DEndpoint);
            var request = new TextTo3DAlphaRequest("Lantern", "game assets", resolution: Resolutions.X1024, artStyle: ArtStyles.Realistic);
            var taskResult = await MeshyClient.TextTo3DEndpoint.CreateTextTo3DTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_CreateTextTo3DTask_Beta()
        {
            Assert.IsNotNull(MeshyClient.TextTo3DEndpoint);
            var request = new TextTo3DBetaPreviewRequest("Lantern", "low poly, ugly", ArtStyles.Realistic);
            var taskResult = await MeshyClient.TextTo3DEndpoint.CreateTextTo3DTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
            Assert.IsTrue(taskResult.Mode == "preview");
            var refineRequest = new TextTo3DBetaRefineRequest(taskResult, TextureRichness.Low);
            var refineResult = await MeshyClient.TextTo3DEndpoint.CreateTextTo3DTaskAsync(refineRequest, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(refineResult);
            Debug.Log($"{refineResult.Id} | created_at: {refineResult.FinishedAt} | expires_at: {refineResult.ExpiresAt}");
        }
    }
}
