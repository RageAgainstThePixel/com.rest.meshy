// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Meshy.ImageTo3D
{
    /// <summary>
    /// Quickly generate high-quality 3d models based on concept art.
    /// </summary>
    public class ImageTo3DEndpoint : MeshyBaseTaskEndpoint
    {
        public ImageTo3DEndpoint(MeshyClient client) : base(client) { }

        protected override string Root => "image-to-3d";
    }
}
