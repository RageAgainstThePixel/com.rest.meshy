// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Utilities.WebRequestRest.Interfaces;

namespace Meshy
{
    public sealed class MeshySettingsInfo : ISettingsInfo
    {
        internal const string DefaultDomain = "api.meshy.ai";
        internal const string DefaultVersion = "v1";

        public MeshySettingsInfo()
        {
            Domain = DefaultDomain;
            BaseRequestUrlFormat = $"https://{Domain}/{DefaultVersion}/{{0}}";
        }

        public MeshySettingsInfo(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = DefaultDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"Invalid parameter \"{nameof(domain)}\"");
            }

            Domain = domain;
            BaseRequestUrlFormat = $"https://{Domain}/{DefaultVersion}/{{0}}";
        }

        public string Domain { get; }

        public string BaseRequestUrlFormat { get; }
    }
}
