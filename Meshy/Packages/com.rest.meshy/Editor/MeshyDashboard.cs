// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.ImageTo3D;
using Meshy.TextTo3D;
using Meshy.TextToTexture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utilities.Extensions.Editor;
using Utilities.WebRequestRest;
using Progress = UnityEditor.Progress;

namespace Meshy.Editor
{
    public class MeshyDashboard : AbstractEditorDashboard
    {
        private static readonly GUIContent generateContent = new("Generate");
        private static readonly GUIContent generationsContent = new("Generations");
        private static readonly GUIContent dashboardTitleContent = new($"{nameof(Meshy)} Dashboard");
        private static readonly string[] tabTitles = { "Text to Texture", "Text to 3D (Beta)", "Text to 3D (Alpha)", "Image to 3D" };
        private static readonly string authError = $"No valid {nameof(MeshyConfiguration)} was found. This tool requires that you set your API key.";

        #region Static Content

        #region Dashboard Overrides

        protected override GUIContent DashboardTitleContent => dashboardTitleContent;

        protected override string DefaultSaveDirectoryKey => $"{Application.productName}_{nameof(Meshy)}_EditorDownloadDirectory";

        protected override string EditorDownloadDirectory { get; set; }

        protected override string[] DashboardTabs => tabTitles;

        protected override bool TryCheckDashboardConfiguration(out string errorMessage)
        {
            errorMessage = authError;
            return api is { HasValidAuthentication: true };
        }

        protected override void RenderTab(int tab)
        {

            switch (tab)
            {
                case 0:
                    RenderTextToTextureTab(tab != lastTab);
                    break;
                case 1:
                    RenderTextTo3DBetaTab(tab != lastTab);
                    break;
                case 2:
                    RenderTextTo3DAlphaTab(tab != lastTab);
                    break;
                case 3:
                    RenderImageTo3DTab(tab != lastTab);
                    break;
            }

            lastTab = tab;
        }

        #endregion Dashboard Overrides

        private static MeshyClient api;

        private static int page = 1;

        private static int limit = 12;

        #endregion Static Content

        [SerializeField]
        private int lastTab;

        [SerializeField]
        private MeshyConfiguration meshyConfiguration;

        private MeshySettings meshySettings;

        private MeshyAuthentication meshyAuthentication;

        [MenuItem("Window/Dashboards/" + nameof(Meshy), false, priority: 999)]
        private static void OpenWindow()
        {
            // Dock it next to the Scene View.
            var instance = GetWindow<MeshyDashboard>(typeof(SceneView));
            instance.Show();
            instance.titleContent = dashboardTitleContent;
        }

        private void OnFocus()
        {
            if (meshyConfiguration == null)
            {
                meshyConfiguration = Resources.Load<MeshyConfiguration>($"{nameof(MeshyConfiguration)}.asset");
            }

            meshyAuthentication ??= meshyConfiguration == null
                ? new MeshyAuthentication().LoadDefaultsReversed()
                : new MeshyAuthentication(meshyConfiguration);
            meshySettings ??= meshyConfiguration == null
                ? new MeshySettings()
                : new MeshySettings(meshyConfiguration);

            api ??= new MeshyClient(meshyAuthentication, meshySettings);
            api.EnableDebug = true;

            EditorApplication.delayCall += () =>
            {
                if (!hasFetchedTextToTextureGenerations)
                {
                    hasFetchedTextToTextureGenerations = true;
                    FetchTextToTextureGenerations();
                }

                if (!hasFetchedTextTo3DBetaGenerations)
                {
                    hasFetchedTextTo3DBetaGenerations = true;
                    FetchTextTo3DBetaGenerations();
                }

                if (!hasFetchedTextTo3DAlphaGenerations)
                {
                    hasFetchedTextTo3DAlphaGenerations = true;
                    FetchTextTo3DAlphaGenerations();
                }

                if (!hasFetchedImageTo3DGenerations)
                {
                    hasFetchedImageTo3DGenerations = true;
                    FetchImageTo3DGenerations();
                }

                Repaint();
            };
        }

        #region Text to Texture

        private void RenderTextToTextureTab(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("AI Texturing", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly generate high-quality textures for 3D models using text prompts and concept art.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderTextToTextureOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderTextToTextureGenerations(refresh);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        [SerializeField]
        private string textToTextureOptions;

        private TextToTextureRequest textToTextureRequest;

        private void RenderTextToTextureOptions()
        {
            textToTextureRequest ??= string.IsNullOrWhiteSpace(textToTextureOptions)
                ? new TextToTextureRequest(string.Empty, string.Empty, string.Empty)
                : JsonConvert.DeserializeObject<TextToTextureRequest>(textToTextureOptions, MeshyClient.JsonSerializationOptions);

            EditorGUI.BeginChangeCheck();

            textToTextureRequest.Model = EditorGUILayout.ObjectField("Model", textToTextureRequest.Model, typeof(GameObject), true) as GameObject;
            textToTextureRequest.ModelUrl = textToTextureRequest.Model == null
                ? EditorGUILayout.TextField("Model Url", textToTextureRequest.ModelUrl)
                : string.Empty;
            textToTextureRequest.ObjectPrompt = EditorGUILayout.DelayedTextField("Prompt", textToTextureRequest.ObjectPrompt);
            textToTextureRequest.StylePrompt = EditorGUILayout.DelayedTextField("Style", textToTextureRequest.StylePrompt);
            textToTextureRequest.NegativePrompt = EditorGUILayout.DelayedTextField("Negative Prompt", textToTextureRequest.NegativePrompt);

            var artStyleSelection = 0;

            for (int i = 0; i < ArtStyles.TextToTextureV1ArtStyles.Length; i++)
            {
                if (textToTextureRequest.ArtStyle == ArtStyles.TextToTextureV1ArtStyles[i])
                {
                    artStyleSelection = i;
                    break;
                }
            }

            artStyleSelection = EditorGUILayout.Popup("Art Style", artStyleSelection, ArtStyles.TextToTextureV1ArtStyles);
            textToTextureRequest.EnableOriginalUV = EditorGUILayout.ToggleLeft("use Original UVs", textToTextureRequest.EnableOriginalUV ?? true);
            textToTextureRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", textToTextureRequest.EnablePBR ?? true);

            var resolutionSelection = 0;

            for (int i = 0; i < Resolutions.ResolutionOptions.Length; i++)
            {
                if (textToTextureRequest.Resolution == Resolutions.ResolutionOptions[i])
                {
                    resolutionSelection = i;
                    break;
                }
            }

            resolutionSelection = EditorGUILayout.Popup("Resolution", resolutionSelection, Resolutions.ResolutionOptions);

            if (EditorGUI.EndChangeCheck())
            {
                textToTextureRequest.ArtStyle = ArtStyles.TextToTextureV1ArtStyles[artStyleSelection];
                textToTextureRequest.Resolution = Resolutions.ResolutionOptions[resolutionSelection];
                textToTextureOptions = JsonConvert.SerializeObject(textToTextureRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(generateContent))
            {
                EditorApplication.delayCall += () =>
                {
                    if (textToTextureRequest.Model == null &&
                        textToTextureRequest.GlbExport == null &&
                        string.IsNullOrWhiteSpace(textToTextureRequest.ModelUrl))
                    {
                        Debug.LogError("Missing required model reference for text to texture task!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(textToTextureRequest.ObjectPrompt))
                    {
                        Debug.LogError("Missing object prompt for text to texture task!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(textToTextureRequest.StylePrompt))
                    {
                        Debug.LogError("Missing style prompt for text to texture task!");
                        return;
                    }

                    GenerateTextToTexture(textToTextureRequest);
                    textToTextureOptions = string.Empty;
                    textToTextureRequest = null;
                };
            }
        }

        private static async void GenerateTextToTexture(TextToTextureRequest request)
        {
            int? progressId = null;
            MeshyTaskResult taskResult = null;
            EditorApplication.LockReloadAssemblies();

            try
            {
                EditorApplication.delayCall += FetchTextToTextureGenerations;
                taskResult = await api.TextToTextureEndpoint.CreateTextToTextureTaskAsync(request, new Progress<TaskProgress>(
                    progress =>
                    {
                        var taskId = progress.Id.ToString("D");

                        if (!progressId.HasValue)
                        {
                            progressId = Progress.Start("Meshy Text to Texture Task", taskId);
                        }
                        else if (Progress.Exists(progressId.Value))
                        {
                            if (progress.PrecedingTasks.HasValue)
                            {
                                Progress.Report(progressId.Value, -1, $"Waiting on {progress.PrecedingTasks.Value} pending tasks");
                            }
                            else
                            {
                                Progress.Report(progressId.Value, progress.Progress * 0.01f, taskId);
                            }
                        }

                        var taskListItem = textToTextureGenerations.FirstOrDefault(task => task.Id == taskId);

                        if (taskListItem != null)
                        {
                            taskListItem.Status = progress.Status;
                            taskListItem.Progress = progress.Progress;
                            taskListItem.PrecedingTasks = progress.PrecedingTasks;
                        }
                    }));
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                if (progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    Progress.Finish(progressId.Value, Progress.Status.Canceled);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();

                if (taskResult != null &&
                    progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    var status = taskResult.Status switch
                    {
                        Status.Succeeded => Progress.Status.Succeeded,
                        _ => Progress.Status.Failed,
                    };

                    Progress.Finish(progressId.Value, status);
                }

                FetchTextToTextureGenerations();
            }
        }

        private Vector2 textToTextureScrollPosition;

        private void RenderTextToTextureGenerations(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(generationsContent, EditorStyles.boldLabel);
            GUI.enabled = !isFetchingTextToTextureGenerations;
            GUILayout.FlexibleSpace();

            if (refresh || GUILayout.Button(RefreshContent, DefaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchTextToTextureGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (textToTextureGenerations == null) { return; }
            textToTextureScrollPosition = EditorGUILayout.BeginScrollView(textToTextureScrollPosition, ExpandWidthOption);

            foreach (var meshyTaskResult in textToTextureGenerations)
            {
                RenderTaskResult(meshyTaskResult);
            }

            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private static bool hasFetchedTextToTextureGenerations;
        private static bool isFetchingTextToTextureGenerations;
        private static IReadOnlyList<MeshyTaskResult> textToTextureGenerations;

        private static async void FetchTextToTextureGenerations()
        {
            if (isFetchingTextToTextureGenerations) { return; }
            isFetchingTextToTextureGenerations = true;
            EditorApplication.LockReloadAssemblies();
            try
            {
                textToTextureGenerations = (await api.TextToTextureEndpoint.ListTasksAsync<TextToTextureRequest>(page, limit, SortOrder.Descending)).Where(task => task.Status != Status.Expired).ToList();
                await Task.WhenAll(textToTextureGenerations.Select(task => task.LoadThumbnailAsync())).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case RestException restEx:
                        if (restEx.Response.Code == 429)
                        {
                            await Task.Delay(1000).ConfigureAwait(true);
                            FetchTextToTextureGenerations();
                        }
                        else
                        {
                            Debug.LogError(restEx);
                        }
                        break;
                    default:
                        Debug.LogError(e);
                        break;
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                isFetchingTextToTextureGenerations = false;
            }
        }

        #endregion Text to Texture

        #region Text to 3d Beta

        private void RenderTextTo3DBetaTab(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Text to 3D (Beta)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly generate impressive 3D models using text prompts.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderTextTo3DBetaPreviewOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderTextTo3DBetaGenerations(refresh);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        [SerializeField]
        private string textTo3DBetaPreviewOptions;

        private TextTo3DBetaPreviewRequest textTo3DBetaPreviewRequest;
        private void RenderTextTo3DBetaPreviewOptions()
        {
            textTo3DBetaPreviewRequest ??= string.IsNullOrWhiteSpace(textTo3DBetaPreviewOptions)
                ? new TextTo3DBetaPreviewRequest(string.Empty, string.Empty)
                : JsonConvert.DeserializeObject<TextTo3DBetaPreviewRequest>(textTo3DBetaPreviewOptions, MeshyClient.JsonSerializationOptions);

            EditorGUI.BeginChangeCheck();

            textTo3DBetaPreviewRequest.Prompt = EditorGUILayout.DelayedTextField("Prompt", textTo3DBetaPreviewRequest.Prompt);
            textTo3DBetaPreviewRequest.NegativePrompt = EditorGUILayout.DelayedTextField("Negative Prompt", textTo3DBetaPreviewRequest.NegativePrompt);

            var artStyleSelection = 0;

            for (int i = 0; i < ArtStyles.TextTo3DV2ArtStyles.Length; i++)
            {
                if (textTo3DBetaPreviewRequest.ArtStyle == ArtStyles.TextTo3DV2ArtStyles[i])
                {
                    artStyleSelection = i;
                    break;
                }
            }

            artStyleSelection = EditorGUILayout.Popup("Art Style", artStyleSelection, ArtStyles.TextTo3DV2ArtStyles);

            if (EditorGUI.EndChangeCheck())
            {
                textTo3DBetaPreviewRequest.ArtStyle = ArtStyles.TextTo3DV2ArtStyles[artStyleSelection];
                textTo3DBetaPreviewOptions = JsonConvert.SerializeObject(textTo3DBetaPreviewRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(generateContent))
            {
                EditorApplication.delayCall += () =>
                {
                    if (string.IsNullOrWhiteSpace(textTo3DBetaPreviewRequest.Prompt))
                    {
                        Debug.LogError("Missing object prompt for text to texture task!");
                        return;
                    }

                    GenerateTextTo3DBetaPreview(textTo3DBetaPreviewRequest);
                    textTo3DBetaPreviewOptions = string.Empty;
                    textTo3DBetaPreviewRequest = null;
                };
            }
        }

        [SerializeField]
        private string textTo3DBetaRefineOptions;

        private TextTo3DBetaRefineRequest textTo3DBetaRefineRequest;


        private void RenderTextTo3DBetaRefineOptions(MeshyTaskResult previewTaskResult)
        {
            textTo3DBetaRefineRequest ??= string.IsNullOrWhiteSpace(textTo3DBetaRefineOptions)
                ? new TextTo3DBetaRefineRequest(previewTaskResult, TextureRichness.Medium)
                : JsonConvert.DeserializeObject<TextTo3DBetaRefineRequest>(textTo3DBetaRefineOptions, MeshyClient.JsonSerializationOptions);

            EditorGUI.BeginChangeCheck();

            textTo3DBetaRefineRequest.TextureRichness = (TextureRichness)EditorGUILayout.EnumPopup("Texture Richness", textTo3DBetaRefineRequest.TextureRichness);

            if (EditorGUI.EndChangeCheck())
            {
                textTo3DBetaRefineOptions = JsonConvert.SerializeObject(textTo3DBetaPreviewRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Refine model"))
            {
                EditorApplication.delayCall += () =>
                {
                    GenerateTextTo3DBetaPreview(textTo3DBetaRefineRequest);
                    textTo3DBetaRefineOptions = string.Empty;
                    textTo3DBetaRefineRequest = null;
                };
            }
        }


        private static async void GenerateTextTo3DBetaPreview(IMeshyTextTo3DRequest request)
        {
            int? progressId = null;
            MeshyTaskResult taskResult = null;
            EditorApplication.LockReloadAssemblies();

            try
            {
                EditorApplication.delayCall += FetchTextTo3DBetaGenerations;
                taskResult = await api.TextTo3DEndpoint.CreateTextTo3DTaskAsync(request, new Progress<TaskProgress>(
                    progress =>
                    {
                        var taskId = progress.Id.ToString("D");

                        if (!progressId.HasValue)
                        {
                            progressId = Progress.Start("Meshy Text to 3D Task", taskId);
                        }
                        else if (Progress.Exists(progressId.Value))
                        {
                            if (progress.PrecedingTasks.HasValue)
                            {
                                Progress.Report(progressId.Value, -1, $"Waiting on {progress.PrecedingTasks.Value} pending tasks");
                            }
                            else
                            {
                                Progress.Report(progressId.Value, progress.Progress * 0.01f, taskId);
                            }
                        }

                        var taskListItem = textTo3DBetaGenerations?.FirstOrDefault(task => task?.Id == taskId);

                        if (taskListItem != null)
                        {
                            taskListItem.Status = progress.Status;
                            taskListItem.Progress = progress.Progress;
                            taskListItem.PrecedingTasks = progress.PrecedingTasks;
                        }
                    }));
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                if (progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    Progress.Finish(progressId.Value, Progress.Status.Failed);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();

                if (taskResult != null &&
                    progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    var status = taskResult.Status switch
                    {
                        Status.Succeeded => Progress.Status.Succeeded,
                        _ => Progress.Status.Failed,
                    };

                    Progress.Finish(progressId.Value, status);
                }

                FetchTextTo3DBetaGenerations();
            }
        }


        private Vector2 textTo3DBetaScrollPosition;

        private void RenderTextTo3DBetaGenerations(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(generationsContent, EditorStyles.boldLabel);
            GUI.enabled = !isFetchingTextTo3DBetaGenerations;
            GUILayout.FlexibleSpace();

            if (refresh || GUILayout.Button(RefreshContent, DefaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchTextTo3DBetaGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (textTo3DBetaGenerations == null) { return; }
            textTo3DBetaScrollPosition = EditorGUILayout.BeginScrollView(textTo3DBetaScrollPosition, ExpandWidthOption);

            foreach (var meshyTaskResult in textTo3DBetaGenerations)
            {
                RenderTaskResult(meshyTaskResult);
            }

            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private static bool hasFetchedTextTo3DBetaGenerations;
        private static bool isFetchingTextTo3DBetaGenerations;
        private static IReadOnlyList<MeshyTaskResult> textTo3DBetaGenerations;

        private static async void FetchTextTo3DBetaGenerations()
        {
            if (isFetchingTextTo3DBetaGenerations) { return; }
            isFetchingTextTo3DBetaGenerations = true;
            EditorApplication.LockReloadAssemblies();
            try
            {
                textTo3DBetaGenerations = (await api.TextTo3DEndpoint.ListTasksAsync<TextTo3DBetaPreviewRequest>(page, limit, SortOrder.Descending)).Where(task => task.Status != Status.Expired).ToList();
                await Task.WhenAll(textTo3DBetaGenerations.Select(task => task.LoadThumbnailAsync())).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case RestException restEx:
                        if (restEx.Response.Code == 429)
                        {
                            await Task.Delay(1000).ConfigureAwait(true);
                            FetchTextTo3DBetaGenerations();
                        }
                        else
                        {
                            Debug.LogError(restEx);
                        }
                        break;
                    default:
                        Debug.LogError(e);
                        break;
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                isFetchingTextTo3DBetaGenerations = false;
            }
        }

        #endregion Text to 3d Beta

        #region Text to 3d Alpha

        private void RenderTextTo3DAlphaTab(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Text to 3D", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly generate impressive 3D models using text prompts.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderTextTo3DAlphaOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderTextTo3DAlphaGenerations(refresh);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        [SerializeField]
        private string textTo3DAlphaOptions;

        private TextTo3DAlphaRequest textTo3DAlphaRequest;

        private void RenderTextTo3DAlphaOptions()
        {
            textTo3DAlphaRequest ??= string.IsNullOrWhiteSpace(textTo3DAlphaOptions)
                ? new TextTo3DAlphaRequest(string.Empty, string.Empty)
                : JsonConvert.DeserializeObject<TextTo3DAlphaRequest>(textTo3DAlphaOptions, MeshyClient.JsonSerializationOptions);

            EditorGUI.BeginChangeCheck();

            textTo3DAlphaRequest.ObjectPrompt = EditorGUILayout.DelayedTextField("Prompt", textTo3DAlphaRequest.ObjectPrompt);
            textTo3DAlphaRequest.StylePrompt = EditorGUILayout.DelayedTextField("Style", textTo3DAlphaRequest.StylePrompt);
            textTo3DAlphaRequest.NegativePrompt = EditorGUILayout.DelayedTextField("Negative Prompt", textTo3DAlphaRequest.NegativePrompt);

            var artStyleSelection = 0;

            for (int i = 0; i < ArtStyles.TextTo3DV1ArtStyles.Length; i++)
            {
                if (textTo3DAlphaRequest.ArtStyle == ArtStyles.TextTo3DV1ArtStyles[i])
                {
                    artStyleSelection = i;
                    break;
                }
            }

            artStyleSelection = EditorGUILayout.Popup("Art Style", artStyleSelection, ArtStyles.TextTo3DV1ArtStyles);
            textTo3DAlphaRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", textTo3DAlphaRequest.EnablePBR ?? true);

            var resolutionSelection = 0;

            for (int i = 0; i < Resolutions.ResolutionOptions.Length; i++)
            {
                if (textTo3DAlphaRequest.Resolution == Resolutions.ResolutionOptions[i])
                {
                    resolutionSelection = i;
                    break;
                }
            }

            resolutionSelection = EditorGUILayout.Popup("Resolution", resolutionSelection, Resolutions.ResolutionOptions);

            if (EditorGUI.EndChangeCheck())
            {
                textTo3DAlphaRequest.ArtStyle = ArtStyles.TextTo3DV1ArtStyles[artStyleSelection];
                textTo3DAlphaRequest.Resolution = Resolutions.ResolutionOptions[resolutionSelection];
                textTo3DAlphaOptions = JsonConvert.SerializeObject(textTo3DAlphaRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(generateContent))
            {
                EditorApplication.delayCall += () =>
                {
                    if (string.IsNullOrWhiteSpace(textTo3DAlphaRequest.ObjectPrompt))
                    {
                        Debug.LogError("Missing object prompt for text to texture task!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(textTo3DAlphaRequest.StylePrompt))
                    {
                        Debug.LogError("Missing style prompt for text to texture task!");
                        return;
                    }

                    GenerateTextTo3DAlpha(textTo3DAlphaRequest);
                    textTo3DAlphaOptions = string.Empty;
                    textTo3DAlphaRequest = null;
                };
            }
        }

        private static async void GenerateTextTo3DAlpha(TextTo3DAlphaRequest request)
        {
            int? progressId = null;
            MeshyTaskResult taskResult = null;
            EditorApplication.LockReloadAssemblies();

            try
            {
                EditorApplication.delayCall += FetchTextTo3DAlphaGenerations;
                taskResult = await api.TextTo3DEndpoint.CreateTextTo3DTaskAsync(request, new Progress<TaskProgress>(
                    progress =>
                    {
                        var taskId = progress.Id.ToString("D");

                        if (!progressId.HasValue)
                        {
                            progressId = Progress.Start("Meshy Text to 3D Task", taskId);
                        }
                        else if (Progress.Exists(progressId.Value))
                        {
                            if (progress.PrecedingTasks.HasValue)
                            {
                                Progress.Report(progressId.Value, -1, $"Waiting on {progress.PrecedingTasks.Value} pending tasks");
                            }
                            else
                            {
                                Progress.Report(progressId.Value, progress.Progress * 0.01f, taskId);
                            }
                        }

                        var taskListItem = textTo3DAlphaGenerations?.FirstOrDefault(task => task?.Id == taskId);

                        if (taskListItem != null)
                        {
                            taskListItem.Status = progress.Status;
                            taskListItem.Progress = progress.Progress;
                            taskListItem.PrecedingTasks = progress.PrecedingTasks;
                        }
                    }));
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                if (progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    Progress.Finish(progressId.Value, Progress.Status.Failed);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();

                if (taskResult != null &&
                    progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    var status = taskResult.Status switch
                    {
                        Status.Succeeded => Progress.Status.Succeeded,
                        _ => Progress.Status.Failed,
                    };

                    Progress.Finish(progressId.Value, status);
                }

                FetchTextTo3DAlphaGenerations();
            }
        }

        private Vector2 textTo3DAlphaScrollPosition;

        private void RenderTextTo3DAlphaGenerations(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(generationsContent, EditorStyles.boldLabel);
            GUI.enabled = !isFetchingTextTo3DAlphaGenerations;
            GUILayout.FlexibleSpace();

            if (refresh || GUILayout.Button(RefreshContent, DefaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchTextTo3DAlphaGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (textTo3DAlphaGenerations == null) { return; }
            textTo3DAlphaScrollPosition = EditorGUILayout.BeginScrollView(textTo3DAlphaScrollPosition, ExpandWidthOption);

            foreach (var meshyTaskResult in textTo3DAlphaGenerations)
            {
                RenderTaskResult(meshyTaskResult);
            }

            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private static bool hasFetchedTextTo3DAlphaGenerations;
        private static bool isFetchingTextTo3DAlphaGenerations;
        private static IReadOnlyList<MeshyTaskResult> textTo3DAlphaGenerations;

        private static async void FetchTextTo3DAlphaGenerations()
        {
            if (isFetchingTextTo3DAlphaGenerations) { return; }
            isFetchingTextTo3DAlphaGenerations = true;
            EditorApplication.LockReloadAssemblies();
            try
            {
                textTo3DAlphaGenerations = (await api.TextTo3DEndpoint.ListTasksAsync<TextTo3DAlphaRequest>(page, limit, SortOrder.Descending)).Where(task => task.Status != Status.Expired).ToList();
                await Task.WhenAll(textTo3DAlphaGenerations.Select(task => task.LoadThumbnailAsync())).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case RestException restEx:
                        if (restEx.Response.Code == 429)
                        {
                            await Task.Delay(1000).ConfigureAwait(true);
                            FetchTextTo3DAlphaGenerations();
                        }
                        else
                        {
                            Debug.LogError(restEx);
                        }
                        break;
                    default:
                        Debug.LogError(e);
                        break;
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                isFetchingTextTo3DAlphaGenerations = false;
            }
        }

        #endregion Text to 3d Alpha

        #region Image to 3d

        private void RenderImageTo3DTab(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Image to 3D", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly transform your 2D images into stunning 3D models and bring your visuals to life.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderImageTo3DOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderImageTo3DGenerations(refresh);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        [SerializeField]
        private string imageTo3DOptions;

        private ImageTo3DRequest imageTo3DRequest;

        private void RenderImageTo3DOptions()
        {
            imageTo3DRequest ??= string.IsNullOrWhiteSpace(imageTo3DOptions)
                ? new ImageTo3DRequest(string.Empty)
                : JsonConvert.DeserializeObject<ImageTo3DRequest>(imageTo3DOptions, MeshyClient.JsonSerializationOptions);

            EditorGUI.BeginChangeCheck();
            // TODO add texture field when api supports it.
            imageTo3DRequest.ImageUrl = EditorGUILayout.DelayedTextField("Image Url", imageTo3DRequest.ImageUrl);
            imageTo3DRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", imageTo3DRequest.EnablePBR ?? true);

            if (EditorGUI.EndChangeCheck())
            {
                imageTo3DOptions = JsonConvert.SerializeObject(imageTo3DRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(generateContent))
            {
                EditorApplication.delayCall += () =>
                {
                    if (imageTo3DRequest.Image == null &&
                        string.IsNullOrWhiteSpace(imageTo3DRequest.ImageUrl))
                    {
                        Debug.LogError("Missing required image reference for image to 3D task!");
                        return;
                    }

                    GenerateImageTo3D(imageTo3DRequest);
                    imageTo3DOptions = string.Empty;
                    imageTo3DRequest = null;
                };
            }
        }

        private static async void GenerateImageTo3D(ImageTo3DRequest request)
        {
            int? progressId = null;
            MeshyTaskResult taskResult = null;
            EditorApplication.LockReloadAssemblies();

            try
            {
                taskResult = await api.ImageTo3DEndpoint.CreateImageTo3DTaskAsync(request, new Progress<TaskProgress>(
                    progress =>
                    {
                        if (!progressId.HasValue)
                        {
                            progressId = Progress.Start("Meshy Image to 3D Task", progress.Id.ToString());
                        }
                        else if (Progress.Exists(progressId.Value))
                        {
                            if (progress.PrecedingTasks.HasValue)
                            {
                                Progress.Report(progressId.Value, -1, $"Pending in Queue. {progress.PrecedingTasks.Value} pending tasks");
                            }
                            else
                            {
                                Progress.Report(progressId.Value, progress.Progress * 0.01f, string.Empty);
                            }
                        }
                    }));
                await taskResult.LoadThumbnailAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                if (progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    Progress.Finish(progressId.Value, Progress.Status.Failed);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();

                if (taskResult != null &&
                    progressId.HasValue &&
                    Progress.Exists(progressId.Value))
                {
                    imageTo3DGenerations.Add(taskResult); // TODO remove this line once FetchImageTo3DGenerations() is Fixed

                    var status = taskResult.Status switch
                    {
                        Status.Succeeded => Progress.Status.Succeeded,
                        _ => Progress.Status.Failed,
                    };

                    Progress.Finish(progressId.Value, status);
                }

                // TODO uncomment after FetchImageTo3DGenerations() is fixed
                // FetchImageTo3DGenerations();
            }
        }

        private Vector2 imageTo3DScrollPosition;

        private void RenderImageTo3DGenerations(bool refresh)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(generationsContent, EditorStyles.boldLabel);
            GUI.enabled = !isFetchingTextTo3DAlphaGenerations;
            GUILayout.FlexibleSpace();

            if (refresh || GUILayout.Button(RefreshContent, DefaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchImageTo3DGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (imageTo3DGenerations == null) { return; }
            imageTo3DScrollPosition = EditorGUILayout.BeginScrollView(imageTo3DScrollPosition, ExpandWidthOption);

            foreach (var meshyTaskResult in imageTo3DGenerations)
            {
                RenderTaskResult(meshyTaskResult);
            }

            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private static bool hasFetchedImageTo3DGenerations;
        private static bool isFetchingImageTo3DGenerations;
        private static readonly List<MeshyTaskResult> imageTo3DGenerations = new();

        private static async void FetchImageTo3DGenerations()
        {
            if (isFetchingImageTo3DGenerations) { return; }
            isFetchingImageTo3DGenerations = true;
            try
            {
                // TODO uncomment after api is fixed. Currently, it is not possible to query for image to 3d tasks.
                //imageTo3DGenerations = (await api.ImageTo3DEndpoint.ListTasksAsync<ImageTo3DRequest>(page, limit, SortOrder.Descending)).Where(task => task.Status != Status.Expired).ToList();
                //await Task.WhenAll(imageTo3DGenerations.Select(task => task.LoadThumbnailAsync())).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case RestException restEx:
                        if (restEx.Response.Code == 429)
                        {
                            await Task.Delay(1000).ConfigureAwait(true);
                            FetchImageTo3DGenerations();
                        }
                        else
                        {
                            Debug.LogError(restEx);
                        }
                        break;
                    default:
                        Debug.LogError(e);
                        break;
                }
            }
            finally
            {
                isFetchingImageTo3DGenerations = false;
            }
        }

        #endregion Image to 3d

        private void RenderTaskResult(MeshyTaskResult meshyTask)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(meshyTask.Id, ExpandWidthOption);

            if (meshyTask.Status == Status.Succeeded &&
                GUILayout.Button(DownloadContent, DefaultColumnWidthOption))
            {
                EditorApplication.delayCall += () =>
                {
                    DownloadTaskAssets(meshyTask);
                };
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (meshyTask.Thumbnail != null)
            {
                EditorGUILayout.ObjectField(meshyTask.Thumbnail, typeof(Texture2D), false, GUILayout.Width(128), GUILayout.Height(128));
            }

            EditorGUILayout.BeginVertical();

            switch (meshyTask.Status)
            {
                case Status.Pending or Status.InProgress when meshyTask.PrecedingTasks.HasValue:
                    EditorGUILayout.LabelField($"Waiting on {meshyTask.PrecedingTasks} preceding tasks...");
                    break;
                case Status.Pending or Status.InProgress:
                    EditorGUILayoutExtensions.DrawProgressBar("Progress", meshyTask.Progress * 0.01f, ExpandWidthOption);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(meshyTask.Prompt))
            {
                EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(meshyTask.Prompt, EditorStyles.wordWrappedLabel);
            }

            if (!string.IsNullOrWhiteSpace(meshyTask.NegativePrompt))
            {
                EditorGUILayout.LabelField("Negative Prompt", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(meshyTask.NegativePrompt, EditorStyles.wordWrappedLabel);
            }

            if (!string.IsNullOrWhiteSpace(meshyTask.ArtStyle))
            {
                EditorGUILayout.LabelField("Art Style", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(meshyTask.ArtStyle, EditorStyles.wordWrappedLabel);
            }

            if (!string.IsNullOrWhiteSpace(meshyTask.StylePrompt))
            {
                EditorGUILayout.LabelField("Style", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(meshyTask.StylePrompt, EditorStyles.wordWrappedLabel);
            }

            if (meshyTask.Mode == "preview")
            {
                RenderTextTo3DBetaRefineOptions(meshyTask);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayoutExtensions.Divider();
        }

        private async void DownloadTaskAssets(MeshyTaskResult meshyTask)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(meshyTask.ModelUrls.Glb))
                {
                    throw new MissingReferenceException("Failed to find a valid model url!");
                }

                var cachedPath = await Rest.DownloadFileAsync(meshyTask.ModelUrls.Glb, fileName: $"{meshyTask.Id}.glb");
                cachedPath = cachedPath.Replace("file://", string.Empty).Replace("/", "\\");

                if (!File.Exists(cachedPath))
                {
                    throw new MissingReferenceException("Failed to download model!");
                }

                if (!Directory.Exists(EditorDownloadDirectory))
                {
                    Directory.CreateDirectory(EditorDownloadDirectory);
                }

                var shallowPath = cachedPath.Replace(Rest.DownloadCacheDirectory.Replace("/", "\\"), string.Empty);
                var importPath = $"{EditorDownloadDirectory}{shallowPath}";

                if (!File.Exists(importPath))
                {
                    File.Copy(cachedPath, importPath);
                    importPath = GetLocalPath(importPath);
                    AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    importPath = GetLocalPath(importPath);
                }

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(importPath);

                if (asset == null)
                {
                    Debug.LogError($"Failed to import asset at {importPath}");
                    return;
                }

                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
