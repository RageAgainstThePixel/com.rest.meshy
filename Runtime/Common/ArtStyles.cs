// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Meshy
{
    public static class ArtStyles
    {
        /// <summary>
        /// Realistic style.
        /// </summary>
        public const string Realistic = "realistic";

        /// <summary>
        ///  Voxel style.
        /// </summary>
        /// <remarks>
        /// Cannot be used with text-to-texture task.
        /// </remarks>
        public const string Voxel = "voxel";

        /// <summary>
        /// 2.5D Cartoon style.
        /// </summary>
        public const string Fake3DCartoon = "fake-3d-cartoon";

        /// <summary>
        /// Japanese Anime style.
        /// </summary>
        public const string JapaneseAnime = "japanese-anime";

        /// <summary>
        /// Cartoon Line Art style.
        /// </summary>
        public const string CartoonLineArt = "cartoon-line-art";

        /// <summary>
        /// Realistic Hand-drawn style.
        /// </summary>
        public const string RealisticHandDrawn = "realistic-hand-drawn";

        /// <summary>
        /// 2.5D Hand-drawn style.
        /// </summary>
        public const string Fake3DHandDrawn = "fake-3d-hand-drawn";

        /// <summary>
        /// Oriental Comic Ink style.
        /// </summary>
        public const string OrientalComicInk = "oriental-comic-ink";

        public const string Cartoon = "cartoon";

        public const string LowPoly = "low-poly";

        internal static readonly string[] TextToTextureV1ArtStyles =
        {
            Realistic,
            Fake3DCartoon,
            JapaneseAnime,
            CartoonLineArt,
            RealisticHandDrawn,
            Fake3DHandDrawn,
            OrientalComicInk
        };

        internal static readonly string[] TextTo3DV2ArtStyles =
        {
            Realistic,
            Cartoon,
            LowPoly
        };

        internal static readonly string[] TextTo3DV1ArtStyles =
        {
            Realistic,
            Voxel,
            Fake3DCartoon,
            JapaneseAnime,
            CartoonLineArt,
            RealisticHandDrawn,
            Fake3DHandDrawn,
            OrientalComicInk
        };
    }
}
