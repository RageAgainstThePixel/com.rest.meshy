// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace Meshy
{
    [CreateAssetMenu(fileName = nameof(MeshyConfiguration), menuName = nameof(Meshy) + "/" + nameof(MeshyConfiguration), order = 0)]
    public sealed class MeshyConfiguration : ScriptableObject, IConfiguration
    {
        [SerializeField]
        internal string apiKey;

        public string ApiKey
        {
            get => apiKey;
            internal set => apiKey = value;
        }

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string proxyDomain;

        public string ProxyDomain => proxyDomain;
    }
}
