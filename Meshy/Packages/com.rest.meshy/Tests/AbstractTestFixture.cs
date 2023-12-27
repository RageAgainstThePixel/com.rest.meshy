// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Meshy.Tests
{
    internal abstract class AbstractTestFixture
    {
        protected readonly MeshyClient MeshyClient;

        protected AbstractTestFixture()
        {
            var auth = new MeshyAuthentication().LoadDefaultsReversed();
            var settings = new MeshySettings();
            MeshyClient = new MeshyClient(auth, settings);
            MeshyClient.EnableDebug = true;
        }
    }
}
