// Licensed under the MIT License. See LICENSE in the project root for license information.

using Meshy.ImageTo3D;
using Meshy.TextTo3D;
using Meshy.TextToTexture;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Authentication;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Utilities.WebRequestRest;

namespace Meshy
{
    public sealed class MeshyClient : BaseClient<MeshyAuthentication, MeshySettings>
    {
        /// <inheritdoc />
        public MeshyClient(MeshyConfiguration configuration)
            : this(
                configuration != null ? new MeshyAuthentication(configuration) : null,
                configuration != null ? new MeshySettings(configuration) : null)
        {
        }

        /// <summary>
        /// Creates a new client for the Meshy API, handling auth and allowing for access to various API endpoints.
        /// </summary>
        /// <param name="authentication">The API authentication information to use for API calls,
        /// or <see langword="null"/> to attempt to use the <see cref="MeshyAuthentication.Default"/>,
        /// potentially loading from environment vars or from a config file.</param>
        /// <param name="settings">Optional, <see cref="MeshySettings"/> for specifying a proxy domain.</param>
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        public MeshyClient(MeshyAuthentication authentication = null, MeshySettings settings = null)
            : base(authentication ?? MeshyAuthentication.Default, settings ?? MeshySettings.Default)
        {
            TextToTextureEndpoint = new TextToTextureEndpoint(this);
            TextTo3DEndpoint = new TextTo3DEndpoint(this);
            ImageTo3DEndpoint = new ImageTo3DEndpoint(this);
        }

        protected override void ValidateAuthentication()
        {
            if (Authentication?.Info == null)
            {
                throw new InvalidCredentialException($"Invalid {nameof(MeshyAuthentication)}");
            }

            if (!HasValidAuthentication)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.meshy#authentication for details.");
            }
        }

        protected override void SetupDefaultRequestHeaders()
        {
            DefaultRequestHeaders = new Dictionary<string, string>
            {
#if !UNITY_WEBGL
                { "User-Agent", "com.rest.meshy" },
#endif
                { "Authorization", Rest.GetBearerOAuthToken(Authentication.Info.ApiKey) }
            };
        }

        public override bool HasValidAuthentication => !string.IsNullOrWhiteSpace(Authentication?.Info?.ApiKey);

        internal static JsonSerializerSettings JsonSerializationOptions { get; } = new()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new SnakeCaseNamingStrategy())
            }
        };

        /// <summary>
        /// Quickly generate high-quality textures for your existing 3D models using text prompts and concept art.
        /// </summary>
        public TextToTextureEndpoint TextToTextureEndpoint { get; }

        /// <summary>
        /// Quickly generate impressive 3D models using text prompts.
        /// </summary>
        public TextTo3DEndpoint TextTo3DEndpoint { get; }

        /// <summary>
        /// Quickly transform your 2D images into stunning 3D models and bring your visuals to life.
        /// </summary>
        public ImageTo3DEndpoint ImageTo3DEndpoint { get; }
    }
}
