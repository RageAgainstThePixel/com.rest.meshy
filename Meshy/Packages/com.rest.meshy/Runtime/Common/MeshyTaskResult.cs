// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.WebRequestRest;

namespace Meshy
{
    [Preserve]
    public sealed class MeshyTaskResult
    {
        [Preserve]
        [JsonConstructor]
        public MeshyTaskResult(
            [JsonProperty("id")] string id,
            [JsonProperty("name")] string name,
            [JsonProperty("mode")] string mode,
            [JsonProperty("model_urls")] ModelUrls modelUrls,
            [JsonProperty("prompt")] string prompt,
            [JsonProperty("object_prompt")] string objectPrompt,
            [JsonProperty("negative_prompt")] string negativePrompt,
            [JsonProperty("art_style")] string artStyle,
            [JsonProperty("style_prompt")] string stylePrompt,
            [JsonProperty("texture_richness")] string textureRichness,
            [JsonProperty("thumbnail_url")] string thumbnailUrl,
            [JsonProperty("progress")] int progress,
            [JsonProperty("started_at")] long startedAtUnixTimeMilliseconds,
            [JsonProperty("created_at")] long createdAtUnitTimeMilliseconds,
            [JsonProperty("expires_at")] long expiresAtUnitTimeMilliseconds,
            [JsonProperty("finished_at")] long finishedAtUnitTimeMilliseconds,
            [JsonProperty("status")] Status status,
            [JsonProperty("task_error")] Error error,
            [JsonProperty("texture_urls")] List<TextureUrl> textureUrls,
            [JsonProperty("preceding_tasks")] int? precedingTasks)
        {
            Id = id;
            Name = name;
            Mode = mode;
            ModelUrls = modelUrls;
            Prompt = string.IsNullOrWhiteSpace(objectPrompt) ? prompt : objectPrompt;
            NegativePrompt = negativePrompt;
            ArtStyle = artStyle;
            StylePrompt = stylePrompt;
            TextureRichness = textureRichness;
            ThumbnailUrl = thumbnailUrl;
            Progress = progress;
            StartedAtUnixTimeMilliseconds = startedAtUnixTimeMilliseconds;
            CreatedAtUnitTimeMilliseconds = createdAtUnitTimeMilliseconds;
            ExpiresAtUnitTimeMilliseconds = expiresAtUnitTimeMilliseconds;
            FinishedAtUnitTimeMilliseconds = finishedAtUnitTimeMilliseconds;
            Status = status;
            Error = error;
            TextureUrls = textureUrls;
            PrecedingTasks = precedingTasks;
        }

        /// <summary>
        /// Unique identifier for the task.
        /// While we use a k-sortable UUID for task ids as the implementation detail,
        /// you should not make any assumptions about the format of the id.
        /// </summary>
        [Preserve]
        [JsonProperty("id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("mode")]
        public string Mode { get; }

        /// <summary>
        /// Downloadable URL to the textured 3D model file generated by Meshy.
        /// </summary>
        [Preserve]
        [JsonProperty("model_urls")]
        public ModelUrls ModelUrls { get; }

        /// <summary>
        /// This is unmodified prompt that was used to create the task.
        /// </summary>
        [Preserve]
        [JsonProperty("prompt")]
        public string Prompt { get; }

        /// <summary>
        /// This is unmodified object_prompt that was used to create the task.
        /// </summary>
        [Preserve]
        [JsonProperty("object_prompt")]
        [Obsolete("use Prompt instead")]
        public string ObjectPrompt => Prompt;

        /// <summary>
        /// This is unmodified style_prompt that was used to create the task.
        /// </summary>
        [Preserve]
        [JsonProperty("style_prompt")]
        public string StylePrompt { get; }

        /// <summary>
        /// This is unmodified negative_prompt that was used to create the task.
        /// </summary>
        [Preserve]
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get; }

        /// <summary>
        /// This is unmodified art_style that was used to create the task.
        /// </summary>
        [Preserve]
        [JsonProperty("art_style")]
        public string ArtStyle { get; }

        /// <summary>
        /// This is unmodified art_style that was used to create the task.
        /// </summary>
        [Preserve]
        [JsonProperty("texture_richness")]
        public string TextureRichness { get; }

        /// <summary>
        /// Downloadable URL to the thumbnail image of the model file.
        /// </summary>
        [Preserve]
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; }

        [Preserve]
        [JsonIgnore]
        public Texture2D Thumbnail { get; private set; }

        /// <summary>
        /// Progress of the task. If the task is not started yet, this property will be 0.
        /// Once the task has succeeded, this will become 100.
        /// </summary>
        [Preserve]
        [JsonProperty("progress")]
        public int Progress { get; internal set; }

        /// <summary>
        /// Timestamp of when the task was started, in milliseconds.
        /// If the task is not started yet, this property will be 0.
        /// </summary>
        [Preserve]
        [JsonProperty("started_at")]
        public long StartedAtUnixTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime StartedAt => DateTimeOffset.FromUnixTimeMilliseconds(StartedAtUnixTimeMilliseconds).DateTime;

        /// <summary>
        /// Timestamp of when the task was created, in milliseconds.
        /// </summary>
        [Preserve]
        [JsonProperty("created_at")]
        public long CreatedAtUnitTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime CreatedAt => DateTimeOffset.FromUnixTimeMilliseconds(CreatedAtUnitTimeMilliseconds).DateTime;

        /// <summary>
        /// Timestamp of when the task result expires, in milliseconds.
        /// </summary>
        [Preserve]
        [JsonProperty("expires_at")]
        public long ExpiresAtUnitTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime ExpiresAt => DateTimeOffset.FromUnixTimeMilliseconds(ExpiresAtUnitTimeMilliseconds).DateTime;

        /// <summary>
        /// Timestamp of when the task was finished, in milliseconds.
        /// If the task is not finished yet, this property will be 0.
        /// </summary>
        [Preserve]
        [JsonProperty("finished_at")]
        public long FinishedAtUnitTimeMilliseconds { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime FinishedAt => DateTimeOffset.FromUnixTimeMilliseconds(FinishedAtUnitTimeMilliseconds).DateTime;

        /// <summary>
        /// Status of the task. Possible values are one of PENDING, IN_PROGRESS, SUCCEEDED, FAILED, EXPIRED.
        /// </summary>
        [Preserve]
        [JsonProperty("status")]
        public Status Status { get; internal set; }

        /// <summary>
        /// Error object that contains the error message if the task failed.
        /// The message property should be empty if the task succeeded.
        /// </summary>
        [Preserve]
        [JsonProperty("task_error")]
        public Error Error { get; }

        /// <summary>
        /// An array of texture URL objects that are generated from the task.
        /// Normally this only contains one texture URL object.
        /// </summary>
        [Preserve]
        [JsonProperty("texture_urls")]
        public IReadOnlyList<TextureUrl> TextureUrls { get; }

        /// <summary>
        /// The count of preceding jobs.
        /// </summary>
        [Preserve]
        [JsonProperty("preceding_tasks")]
        public int? PrecedingTasks { get; internal set; }

        [Preserve]
        public static implicit operator string(MeshyTaskResult result) => result?.ToString();

        [Preserve]
        public override string ToString() => Id;

        [Preserve]
        public async Task<Texture2D> LoadThumbnailAsync(bool enableDebug = false, CancellationToken cancellationToken = default)
        {
            if (Thumbnail == null && !string.IsNullOrWhiteSpace(ThumbnailUrl))
            {
                Thumbnail = await Rest.DownloadTextureAsync(ThumbnailUrl, fileName: $"{Id}.png", parameters: new RestParameters(debug: enableDebug), cancellationToken: cancellationToken);
            }

            return Thumbnail;
        }
    }
}
