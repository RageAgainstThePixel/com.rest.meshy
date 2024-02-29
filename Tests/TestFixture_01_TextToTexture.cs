// Licensed under the MIT License. See LICENSE in the project root for license information.

using GLTFast;
using GLTFast.Export;
using Meshy.TextToTexture;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Meshy.Tests
{
    internal class TestFixture_01_TextToTexture : AbstractTestFixture
    {
        private readonly string testGlbUrl = "https://github.com/KhronosGroup/UnityGLTF/raw/master/UnityGLTF/Assets/StreamingAssets/Lantern/glTF-Binary/Lantern.glb";

        [Test]
        public async Task Test_01_GetTextToTextureTasks()
        {
            Assert.IsNotNull(MeshyClient.TextToTextureEndpoint);
            var textToTextureTasks = await MeshyClient.TextToTextureEndpoint.ListTasksAsync<TextToTextureRequest>(1, 12, SortOrder.Ascending);
            Assert.IsNotNull(textToTextureTasks);

            foreach (var meshyTask in textToTextureTasks)
            {
                Debug.Log($"{meshyTask.Id} | created_at: {meshyTask.CreatedAt}");
            }
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_01_CreateTextToTextureTask_URL()
        {
            Assert.IsNotNull(MeshyClient.TextToTextureEndpoint);
            var request = new TextToTextureRequest(testGlbUrl, "Lantern", "game assets", resolution: Resolutions.X1024, artStyle: ArtStyles.Realistic);
            var taskResult = await MeshyClient.TextToTextureEndpoint.CreateTextToTextureTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_02_CreateTextToTextureTask_Prefab()
        {
            Assert.IsNotNull(MeshyClient.TextToTextureEndpoint);
            var assetPath = AssetDatabase.GUIDToAssetPath("eeb12f5b14a32694a84aa89d0728bf0f");
            var spherePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            var request = new TextToTextureRequest(spherePrefab, "Basketball", "game assets", enableOriginalUV: false, resolution: Resolutions.X1024, artStyle: ArtStyles.Realistic);
            var taskResult = await MeshyClient.TextToTextureEndpoint.CreateTextToTextureTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }

        [Test]
        [Timeout(720000)]
        public async Task Test_02_03_CreateTextToTexture_GlbExport()
        {
            Assert.IsNotNull(MeshyClient.TextToTextureEndpoint);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var exportSettings = new ExportSettings
            {
                Format = GltfFormat.Binary,
                FileConflictResolution = FileConflictResolution.Overwrite,
                ComponentMask = ~(ComponentType.Camera | ComponentType.Animation | ComponentType.Light),
            };
            var gameObjectExportSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = false,
                DisabledComponents = true,
            };
            var glbExport = new GameObjectExport(exportSettings, gameObjectExportSettings);
            glbExport.AddScene(new[] { sphere });
            var request = new TextToTextureRequest(glbExport, "Planet", "game asset, space, vibrant, highly detailed", enableOriginalUV: false, resolution: Resolutions.X1024, artStyle: ArtStyles.Realistic);
            var taskResult = await MeshyClient.TextToTextureEndpoint.CreateTextToTextureTaskAsync(request, new Progress<TaskProgress>(progress => Debug.Log($"[{progress.Id}] {progress.Status}: {progress.PrecedingTasks ?? progress.Progress}")));
            Assert.IsNotNull(taskResult);
            Debug.Log($"{taskResult.Id} | created_at: {taskResult.FinishedAt} | expires_at: {taskResult.ExpiresAt}");
        }
    }
}
