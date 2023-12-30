// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace Meshy
{
    public sealed class MeshySettings : ISettings
    {
        /// <summary>
        /// Creates a new instance of <see cref="MeshySettings"/> with default <see cref="MeshySettingsInfo"/>.
        /// </summary>
        public MeshySettings()
        {
            Info = new MeshySettingsInfo();
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeshySettings"/> with provided <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="MeshyConfiguration"/>.</param>
        public MeshySettings(MeshyConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = Resources.Load<MeshyConfiguration>($"{nameof(MeshyConfiguration)}.asset");
            }

            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed up by directly passing a {nameof(MeshyConfiguration)} to the {nameof(MeshySettings)}.ctr");
                configuration = Resources.LoadAll<MeshyConfiguration>(string.Empty).FirstOrDefault(asset => asset != null);
            }

            if (configuration != null)
            {
                Info = new MeshySettingsInfo(configuration.ProxyDomain);
                cachedDefault = this;
            }
            else
            {
                Info = new MeshySettingsInfo();
                cachedDefault = this;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeshySettings"/> with the provided <see cref="settingsInfo"/>.
        /// </summary>
        /// <param name="settingsInfo"><see cref="MeshySettingsInfo"/>.</param>
        public MeshySettings(MeshySettingsInfo settingsInfo)
        {
            Info = settingsInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeshySettings"/>.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        public MeshySettings(string domain)
        {
            Info = new MeshySettingsInfo(domain);
            cachedDefault = this;
        }

        private static MeshySettings cachedDefault;

        public static MeshySettings Default
        {
            get => cachedDefault ??= new MeshySettings(configuration: null);
            internal set => cachedDefault = value;
        }

        public MeshySettingsInfo Info { get; }

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}
