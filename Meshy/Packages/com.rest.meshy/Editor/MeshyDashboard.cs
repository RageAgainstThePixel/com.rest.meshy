// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meshy.ImageTo3D;
using Newtonsoft.Json;
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
            EditorGUILayoutExtensions.Divider();
            RenderTextToTextureGenerations();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        private void RenderTextToTextureOptions()
        {

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
            EditorGUILayoutExtensions.Divider();
            RenderTextTo3DGenerations();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        private void RenderTextTo3DOptions()
        {
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
            imageTo3DRequest.ImageUrl = EditorGUILayout.DelayedTextField("Image Url", imageTo3DRequest.ImageUrl);
            imageTo3DRequest.EnablePBR = EditorGUILayout.ToggleLeft("Enable PBR", imageTo3DRequest.EnablePBR!.Value);

            if (EditorGUI.EndChangeCheck())
            {
                imageTo3DOptions = JsonConvert.SerializeObject(imageTo3DRequest, MeshyClient.JsonSerializationOptions);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate"))
            {
                EditorApplication.delayCall += () =>
                {
                    GenerateImageTo3D(imageTo3DRequest);
                    imageTo3DOptions = string.Empty;
                    imageTo3DRequest = null;
                };
            }
        }

        private async void GenerateImageTo3D(ImageTo3DRequest request)
        {
            int? progressId = null;
            MeshyTaskResult taskResult = null;

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
                                    Progress.Report(progressId.Value, -1, $"Waiting on {progress.PrecedingTasks.Value} pending tasks");
                                }
                                else
                                {
                                    Progress.Report(progressId.Value, progress.Progress * 0.01f);
                                }
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

        private static void RenderTaskResult(MeshyTaskResult textToTextureTask)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(textToTextureTask.Id);

            if (textToTextureTask.Progress < 100)
            {
                EditorGUILayout.LabelField($"Progress: {textToTextureTask.Progress}");
            }
            else
            {
                if (GUILayout.Button(downloadContent, defaultColumnWidthOption))
                {
                    // TODO
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
