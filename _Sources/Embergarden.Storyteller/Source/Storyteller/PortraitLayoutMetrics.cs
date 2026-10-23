using System.Collections.Generic;
using UnityEngine;

namespace Embergarden.Storytelling
{
    public static class PortraitLayoutMetrics
    {
        private const byte MinimumVisibleAlpha = 8;
        private static readonly Dictionary<Texture2D, float> Cache =
            new Dictionary<Texture2D, float>();

        public static float OccupiedRightFraction(Texture2D texture)
        {
            if (texture == null) return 1f;
            if (Cache.TryGetValue(texture, out float cached)) return cached;
            float fraction;
            try
            {
                fraction = CalculateOccupiedRightFraction(texture.GetPixels32(),
                    texture.width, texture.height);
            }
            catch
            {
                fraction = 1f;
            }
            Cache[texture] = fraction;
            return fraction;
        }

        public static float CalculateOccupiedRightFraction(Color32[] pixels,
            int width, int height)
        {
            if (pixels == null || width <= 0 || height <= 0 || pixels.Length < width * height)
                return 1f;
            int rightmost = -1;
            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = width - 1; x > rightmost; x--)
                {
                    if (pixels[row + x].a < MinimumVisibleAlpha) continue;
                    rightmost = x;
                    break;
                }
                if (rightmost == width - 1) break;
            }
            return rightmost < 0 ? 1f : (rightmost + 1f) / width;
        }
    }
}
