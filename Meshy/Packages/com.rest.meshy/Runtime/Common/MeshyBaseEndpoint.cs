// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Utilities.WebRequestRest;

namespace Meshy
{
    public abstract class MeshyBaseEndpoint : BaseEndPoint<MeshyClient, MeshyAuthentication, MeshySettings>
    {
        protected MeshyBaseEndpoint(MeshyClient client) : base(client) { }
    }
}
