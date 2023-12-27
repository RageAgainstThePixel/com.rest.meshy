// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security.Authentication;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace Meshy
{
    [Serializable]
    public sealed class MeshyAuthInfo : IAuthInfo
    {
        internal const string SecretKeyPrefix = "msy_";

        public MeshyAuthInfo(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidCredentialException(nameof(apiKey));
            }

            this.apiKey = apiKey;
        }

        [SerializeField]
        private string apiKey;

        /// <summary>
        /// The API key, required to access the service.
        /// </summary>
        public string ApiKey => apiKey;
    }
}
