// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.ImageTo3D;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_03_ImageTo3D : AbstractTestFixture
    {
        private readonly string testImageUrl = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/Fox/screenshot/screenshot-x150.jpg";

        [Test]
        public async Task Test_01_GetImageTo3DTasks()
        {
            Assert.IsNotNull(MeshyClient.ImageTo3DEndpoint);
            var imageTo3dTasks = await MeshyClient.ImageTo3DEndpoint.ListTasksAsync();
            Assert.IsNotNull(imageTo3dTasks);

            foreach (var meshyTask in imageTo3dTasks)
            {
                Debug.Log($"{meshyTask.Id} | created_at: {meshyTask.CreatedAt}");
            }
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_01_CreateImageTo3DTask_URL()
        {
            Assert.IsNotNull(MeshyClient.ImageTo3DEndpoint);
            var request = new ImageTo3DRequest(testImageUrl);
            var taskResult = await MeshyClient.ImageTo3DEndpoint.CreateImageTo3DTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_02_CreateImageTo3DTask_Texture()
        {
            Assert.IsNotNull(MeshyClient.ImageTo3DEndpoint);
            var assetPath = AssetDatabase.GUIDToAssetPath("5ed98df93c3e71647a91873feef5a631");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            var request = new ImageTo3DRequest(texture);
            var taskResult = await MeshyClient.ImageTo3DEndpoint.CreateImageTo3DTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }
    }
}
