// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace Meshy
{
    public sealed class MeshyAuthentication : AbstractAuthentication<MeshyAuthentication, MeshyAuthInfo, MeshyConfiguration>
    {
        internal const string CONFIG_FILE = ".meshy";
        private const string MESHY_API_KEY = nameof(MESHY_API_KEY);

        public static implicit operator MeshyAuthentication(string apiKey) => new MeshyAuthentication(apiKey);

        /// <summary>
        /// Instantiates an empty Authentication object.
        /// </summary>
        public MeshyAuthentication() { }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="apiKey"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="apiKey">The API key, required to access the API endpoint.</param>
        public MeshyAuthentication(string apiKey)
        {
            Info = new MeshyAuthInfo(apiKey);
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="authInfo"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="authInfo"></param>
        public MeshyAuthentication(MeshyAuthInfo authInfo)
        {
            Info = authInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="MeshyConfiguration"/>.</param>
        public MeshyAuthentication(MeshyConfiguration configuration) : this(configuration.ApiKey) { }

        /// <inheritdoc />
        public override MeshyAuthInfo Info { get; }

        private static MeshyAuthentication cachedDefault;

        /// <summary>
        /// The default authentication to use when no other auth is specified.
        /// This can be set manually, or automatically loaded via environment variables or a config file.
        /// <seealso cref="LoadFromEnvironment"/><seealso cref="LoadFromDirectory"/>
        /// </summary>
        public static MeshyAuthentication Default
        {
            get => cachedDefault ??= new MeshyAuthentication().LoadDefault();
            set => cachedDefault = value;
        }

        /// <inheritdoc />
        public override MeshyAuthentication LoadFromAsset(MeshyConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = Resources.Load<MeshyConfiguration>($"{nameof(MeshyConfiguration)}.asset");
            }

            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed up by passing a {nameof(MeshyConfiguration)} to the {nameof(MeshyAuthentication)}.ctr");
                configuration = Resources.LoadAll<MeshyConfiguration>(string.Empty).FirstOrDefault(o => o != null);
            }

            return configuration != null ? new MeshyAuthentication(configuration) : null;
        }

        /// <inheritdoc />
        public override MeshyAuthentication LoadFromEnvironment()
        {
            var apiKey = Environment.GetEnvironmentVariable(MESHY_API_KEY);
            return string.IsNullOrEmpty(apiKey) ? null : new MeshyAuthentication(apiKey);
        }

        /// <inheritdoc />
        /// ReSharper disable once OptionalParameterHierarchyMismatch
        public override MeshyAuthentication LoadFromDirectory(string directory = null, string filename = CONFIG_FILE, bool searchUp = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = CONFIG_FILE;
            }

            MeshyAuthInfo tempAuthInfo = null;

            var currentDirectory = new DirectoryInfo(directory);

            while (tempAuthInfo == null && currentDirectory.Parent != null)
            {
                var filePath = Path.Combine(currentDirectory.FullName, filename);

                if (File.Exists(filePath))
                {
                    try
                    {
                        tempAuthInfo = JsonUtility.FromJson<MeshyAuthInfo>(File.ReadAllText(filePath));
                        break;
                    }
                    catch (Exception)
                    {
                        // try to parse the old way for backwards support.
                    }

                    var lines = File.ReadAllLines(filePath);
                    string apiKey = null;

                    foreach (var line in lines)
                    {
                        var parts = line.Split('=', ':');

                        for (var i = 0; i < parts.Length - 1; i++)
                        {
                            var part = parts[i];
                            var nextPart = parts[i + 1];

                            apiKey = part switch
                            {
                                MESHY_API_KEY => nextPart.Trim(),
                                _ => apiKey
                            };
                        }
                    }

                    tempAuthInfo = new MeshyAuthInfo(apiKey);
                }

                if (searchUp)
                {
                    currentDirectory = currentDirectory.Parent;
                }
                else
                {
                    break;
                }
            }

            return string.IsNullOrEmpty(tempAuthInfo?.ApiKey) ? null : new MeshyAuthentication(tempAuthInfo);
        }
    }
}
