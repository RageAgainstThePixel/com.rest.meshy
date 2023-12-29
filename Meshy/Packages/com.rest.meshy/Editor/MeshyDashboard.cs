// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.ImageTo3D;
using Meshy.TextTo3D;
using Meshy.TextToTexture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utilities.Extensions.Editor;
using Utilities.WebRequestRest;
using Progress = UnityEditor.Progress;

namespace Meshy.Editor
{
    public class MeshyDashboard : EditorWindow
    {
        private const int TabWidth = 18;
        private const int EndWidth = 10;
        private const int MaxCharacterLength = 5000;
        private const int InnerLabelIndentLevel = 13;

        private const float InnerLabelWidth = 1.9f;
        private const float WideColumnWidth = 128f;
        private const float DefaultColumnWidth = 96f;
        private const float SettingsLabelWidth = 1.56f;

        private static readonly GUIContent resetContent = new GUIContent("Reset");
        private static readonly GUIContent deleteContent = new GUIContent("Delete");
        private static readonly GUIContent refreshContent = new GUIContent("Refresh");
        private static readonly GUIContent downloadContent = new GUIContent("Download");
        private static readonly GUIContent generateContent = new GUIContent("Generate");
        private static readonly GUIContent saveDirectoryContent = new GUIContent("Save Directory");
        private static readonly GUIContent changeDirectoryContent = new GUIContent("Change Save Directory");
        private static readonly GUIContent dashboardTitleContent = new GUIContent($"{nameof(Meshy)} Dashboard");

        private static readonly GUILayoutOption[] defaultColumnWidthOption =
        {
            GUILayout.Width(DefaultColumnWidth)
        };

        private static readonly GUILayoutOption[] wideColumnWidthOption =
        {
            GUILayout.Width(WideColumnWidth)
        };

        private static readonly GUILayoutOption[] expandWidthOption =
        {
            GUILayout.ExpandWidth(true)
        };

        private static readonly string[] tabTitles = { "Text to Texture", "Text to 3D", "Image to 3D" };

        private static GUIStyle boldCenteredHeaderStyle;

        private static GUIStyle BoldCenteredHeaderStyle
        {
            get
            {
                if (boldCenteredHeaderStyle == null)
                {
                    var editorStyle = EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel;

                    if (editorStyle != null)
                    {
                        boldCenteredHeaderStyle = new GUIStyle(editorStyle)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                            padding = new RectOffset(0, 0, -8, -8)
                        };
                    }
                }

                return boldCenteredHeaderStyle;
            }
        }

        private static string DefaultSaveDirectoryKey => $"{Application.productName}_{nameof(Meshy)}_EditorDownloadDirectory";

        private static string DefaultSaveDirectory => Application.dataPath;

        #region Static Content

        private static MeshyClient api;

        private static string editorDownloadDirectory = string.Empty;

        private static int page = 1;

        private static int limit = 12;

        #endregion Static Content

        [SerializeField]
        private int tab;

        private Vector2 scrollPosition = Vector2.zero;

        [MenuItem("Window/Dashboards/" + nameof(Meshy), false, priority: 999)]
        private static void OpenWindow()
        {
            // Dock it next to the Scene View.
            var instance = GetWindow<MeshyDashboard>(typeof(SceneView));
            instance.Show();
            instance.titleContent = dashboardTitleContent;
        }

        private void OnEnable()
        {
            titleContent = dashboardTitleContent;
            minSize = new Vector2(WideColumnWidth * 4.375F, WideColumnWidth * 4);
        }

        private void OnFocus()
        {
            api ??= new MeshyClient(new MeshyAuthentication().LoadDefaultsReversed());

            if (!hasFetchedTextToTextureGenerations)
            {
                hasFetchedTextToTextureGenerations = true;
                FetchTextToTextureGenerations();
            }

            if (!hasFetchedTextTo3DGenerations)
            {
                hasFetchedTextTo3DGenerations = true;
                FetchTextTo3DGenerations();
            }

            if (!hasFetchedImageTo3DGenerations)
            {
                hasFetchedImageTo3DGenerations = true;
                FetchImageTo3DGenerations();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            EditorGUILayout.BeginVertical();
            { // Begin Header
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(dashboardTitleContent, BoldCenteredHeaderStyle);
                EditorGUILayout.Space();

                if (api is not { HasValidAuthentication: true })
                {
                    EditorGUILayout.HelpBox($"No valid {nameof(MeshyConfiguration)} was found. This tool requires that you set your API key.", MessageType.Error);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                tab = GUILayout.Toolbar(tab, tabTitles, expandWidthOption);

                if (EditorGUI.EndChangeCheck())
                {
                    GUI.FocusControl(null);
                    // reset generation list page.
                    page = 1;
                }

                EditorGUILayout.LabelField(saveDirectoryContent);

                if (string.IsNullOrWhiteSpace(editorDownloadDirectory))
                {
                    editorDownloadDirectory = EditorPrefs.GetString(DefaultSaveDirectoryKey, DefaultSaveDirectory);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.TextField(editorDownloadDirectory, expandWidthOption);

                    if (GUILayout.Button(resetContent, wideColumnWidthOption))
                    {
                        editorDownloadDirectory = DefaultSaveDirectory;
                        EditorPrefs.SetString(DefaultSaveDirectoryKey, editorDownloadDirectory);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(changeDirectoryContent, expandWidthOption))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            var result = EditorUtility.OpenFolderPanel(saveDirectoryContent.text, editorDownloadDirectory, string.Empty);

                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                editorDownloadDirectory = result;
                                EditorPrefs.SetString(DefaultSaveDirectoryKey, editorDownloadDirectory);
                            }
                        };
                    }
                }
                EditorGUILayout.EndHorizontal();
            } // End Header
            EditorGUILayout.EndVertical();
            GUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, expandWidthOption);
            EditorGUI.indentLevel++;

            switch (tab)
            {
                case 0:
                    RenderTextToTextureTab();
                    break;
                case 1:
                    RenderTextTo3DTab();
                    break;
                case 2:
                    RenderImageTo3DTab();
                    break;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #region Text to Texture

        private void RenderTextToTextureTab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("AI Texturing", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly generate high-quality textures for 3D models using text prompts and concept art.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderTextToTextureOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderTextToTextureGenerations();
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
            textToTextureRequest.ObjectPrompt = EditorGUILayout.DelayedTextField("Object", textToTextureRequest.ObjectPrompt);
            textToTextureRequest.StylePrompt = EditorGUILayout.DelayedTextField("Style", textToTextureRequest.StylePrompt);
            textToTextureRequest.NegativePrompt = EditorGUILayout.DelayedTextField("Negative Prompt", textToTextureRequest.NegativePrompt);

            var artStyleSelection = 0;

            for (int i = 0; i < ArtStyles.TextToImageArtStyles.Length; i++)
            {
                if (textToTextureRequest.ArtStyle == ArtStyles.TextToImageArtStyles[i])
                {
                    artStyleSelection = i;
                    break;
                }
            }

            artStyleSelection = EditorGUILayout.Popup("Art Style", artStyleSelection, ArtStyles.TextToImageArtStyles);
            textToTextureRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", textToTextureRequest.EnablePBR!.Value);

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
                textToTextureRequest.ArtStyle = ArtStyles.TextToImageArtStyles[artStyleSelection];
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

        private void RenderTextToTextureGenerations()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generations");
            GUI.enabled = !isFetchingTextToTextureGenerations;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(refreshContent, defaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchTextToTextureGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (textToTextureGenerations == null) { return; }
            textToTextureScrollPosition = EditorGUILayout.BeginScrollView(textToTextureScrollPosition, expandWidthOption);

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
            try
            {
                textToTextureGenerations = await api.TextToTextureEndpoint.ListTasksAsync(page, limit, SortOrder.Descending);
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
                isFetchingTextToTextureGenerations = false;
            }
        }

        #endregion Text to Texture

        #region Text to 3d

        private void RenderTextTo3DTab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Text to 3D", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly generate impressive 3D models using text prompts.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderTextTo3DOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderTextTo3DGenerations();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        [SerializeField]
        private string textTo3DOptions;

        private TextTo3DRequest textTo3DRequest;

        private void RenderTextTo3DOptions()
        {
            textTo3DRequest ??= string.IsNullOrWhiteSpace(textTo3DOptions)
                ? new TextTo3DRequest(string.Empty, string.Empty)
                : JsonConvert.DeserializeObject<TextTo3DRequest>(textTo3DOptions, MeshyClient.JsonSerializationOptions);

            EditorGUI.BeginChangeCheck();

            textTo3DRequest.ObjectPrompt = EditorGUILayout.DelayedTextField("Object", textTo3DRequest.ObjectPrompt);
            textTo3DRequest.StylePrompt = EditorGUILayout.DelayedTextField("Style", textTo3DRequest.StylePrompt);
            textTo3DRequest.NegativePrompt = EditorGUILayout.DelayedTextField("Negative Prompt", textTo3DRequest.NegativePrompt);

            var artStyleSelection = 0;

            for (int i = 0; i < ArtStyles.TextToImageArtStyles.Length; i++)
            {
                if (textTo3DRequest.ArtStyle == ArtStyles.TextToImageArtStyles[i])
                {
                    artStyleSelection = i;
                    break;
                }
            }

            artStyleSelection = EditorGUILayout.Popup("Art Style", artStyleSelection, ArtStyles.TextToImageArtStyles);
            textTo3DRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", textTo3DRequest.EnablePBR!.Value);

            var resolutionSelection = 0;

            for (int i = 0; i < Resolutions.ResolutionOptions.Length; i++)
            {
                if (textTo3DRequest.Resolution == Resolutions.ResolutionOptions[i])
                {
                    resolutionSelection = i;
                    break;
                }
            }

            resolutionSelection = EditorGUILayout.Popup("Resolution", resolutionSelection, Resolutions.ResolutionOptions);

            if (EditorGUI.EndChangeCheck())
            {
                textTo3DRequest.ArtStyle = ArtStyles.TextToImageArtStyles[artStyleSelection];
                textTo3DRequest.Resolution = Resolutions.ResolutionOptions[resolutionSelection];
                textTo3DOptions = JsonConvert.SerializeObject(textTo3DRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(generateContent))
            {
                EditorApplication.delayCall += () =>
                {
                    if (string.IsNullOrWhiteSpace(textTo3DRequest.ObjectPrompt))
                    {
                        Debug.LogError("Missing object prompt for text to texture task!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(textTo3DRequest.StylePrompt))
                    {
                        Debug.LogError("Missing style prompt for text to texture task!");
                        return;
                    }

                    GenerateTextTo3D(textTo3DRequest);
                    textTo3DOptions = string.Empty;
                    textTo3DRequest = null;
                };
            }
        }

        private static async void GenerateTextTo3D(TextTo3DRequest request)
        {
            int? progressId = null;
            MeshyTaskResult taskResult = null;
            EditorApplication.LockReloadAssemblies();

            try
            {
                taskResult = await api.TextTo3DEndpoint.CreateTextTo3DTaskAsync(request, new Progress<TaskProgress>(
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

                        var taskListItem = textTo3DGenerations.FirstOrDefault(task => task.Id == taskId);

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

                FetchTextTo3DGenerations();
            }
        }

        private Vector2 textTo3DScrollPosition;

        private void RenderTextTo3DGenerations()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generations");
            GUI.enabled = !isFetchingTextTo3DGenerations;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(refreshContent, defaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchTextTo3DGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (textTo3DGenerations == null) { return; }
            textTo3DScrollPosition = EditorGUILayout.BeginScrollView(textTo3DScrollPosition, expandWidthOption);

            foreach (var meshyTaskResult in textTo3DGenerations)
            {
                RenderTaskResult(meshyTaskResult);
            }

            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private static bool hasFetchedTextTo3DGenerations;
        private static bool isFetchingTextTo3DGenerations;
        private static IReadOnlyList<MeshyTaskResult> textTo3DGenerations;

        private static async void FetchTextTo3DGenerations()
        {
            if (isFetchingTextTo3DGenerations) { return; }
            isFetchingTextTo3DGenerations = true;
            try
            {
                textTo3DGenerations = await api.TextTo3DEndpoint.ListTasksAsync(page, limit, SortOrder.Descending);
                await Task.WhenAll(textTo3DGenerations.Select(task => task.LoadThumbnailAsync())).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case RestException restEx:
                        if (restEx.Response.Code == 429)
                        {
                            await Task.Delay(1000).ConfigureAwait(true);
                            FetchTextTo3DGenerations();
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
                isFetchingTextTo3DGenerations = false;
            }
        }

        #endregion Text to 3d

        #region Image to 3d

        private void RenderImageTo3DTab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Image to 3D", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quickly transform your 2D images into stunning 3D models and bring your visuals to life.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            RenderImageTo3DOptions();
            EditorGUILayout.Space();
            EditorGUILayoutExtensions.Divider();
            RenderImageTo3DGenerations();
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
            imageTo3DRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", imageTo3DRequest.EnablePBR!.Value);

            if (EditorGUI.EndChangeCheck())
            {
                imageTo3DOptions = JsonConvert.SerializeObject(imageTo3DRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(generateContent))
            {
                EditorApplication.delayCall += () =>
                {
                    if (imageTo3DRequest.Image == null ||
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
                    imageTo3DGenerations.Add(taskResult); // TODO remove this line once api is fixed and we can properly list generations.

                    var status = taskResult.Status switch
                    {
                        Status.Succeeded => Progress.Status.Succeeded,
                        _ => Progress.Status.Failed,
                    };

                    Progress.Finish(progressId.Value, status);
                }

                // TODO fetch image to 3d generations after api is fixed.
            }
        }

        private Vector2 imageTo3DScrollPosition;

        private void RenderImageTo3DGenerations()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generations");
            GUI.enabled = !isFetchingTextTo3DGenerations;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(refreshContent, defaultColumnWidthOption))
            {
                EditorApplication.delayCall += FetchTextTo3DGenerations;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (imageTo3DGenerations == null) { return; }
            imageTo3DScrollPosition = EditorGUILayout.BeginScrollView(imageTo3DScrollPosition, expandWidthOption);

            foreach (var meshyTaskResult in imageTo3DGenerations)
            {
                RenderTaskResult(meshyTaskResult);
            }

            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private static bool hasFetchedImageTo3DGenerations;
        private static bool isFetchingImageTo3DGenerations;
        private static List<MeshyTaskResult> imageTo3DGenerations = new List<MeshyTaskResult>();

        private static async void FetchImageTo3DGenerations()
        {
            if (isFetchingImageTo3DGenerations) { return; }
            isFetchingImageTo3DGenerations = true;
            try
            {
                // TODO BUG uncomment after api is fixed.
                // imageTo3DGenerations = await api.ImageTo3DEndpoint.ListTasksAsync(page, limit, SortOrder.Descending);
                await Task.CompletedTask;
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

        private static void RenderTaskResult(MeshyTaskResult meshyTask)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(meshyTask.Id, expandWidthOption);

            if (GUILayout.Button(downloadContent, defaultColumnWidthOption))
            {
                // TODO
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (meshyTask.Thumbnail != null)
            {
                EditorGUILayout.ObjectField(meshyTask.Thumbnail, typeof(Texture2D), false, GUILayout.Width(128), GUILayout.Height(128));
            }

            EditorGUILayout.BeginVertical();

            if (meshyTask.Progress < 100)
            {
                if (meshyTask.PrecedingTasks.HasValue)
                {
                    EditorGUILayout.LabelField($"Waiting on {meshyTask.PrecedingTasks} preceding tasks...");
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Progress:");
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 18f, expandWidthOption), meshyTask.Progress * 0.01f, $"{meshyTask.Progress}%");
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (!string.IsNullOrWhiteSpace(meshyTask.ObjectPrompt))
            {
                EditorGUILayout.LabelField("Object", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(meshyTask.ObjectPrompt, EditorStyles.wordWrappedLabel);
            }

            if (!string.IsNullOrWhiteSpace(meshyTask.StylePrompt))
            {
                EditorGUILayout.LabelField("Style", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(meshyTask.StylePrompt, EditorStyles.wordWrappedLabel);
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

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayoutExtensions.Divider();
        }
    }
}