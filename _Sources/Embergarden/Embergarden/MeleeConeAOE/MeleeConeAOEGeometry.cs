using System;

namespace Embergarden
{
    internal static class MeleeConeAOEGeometry
    {
        internal static bool ContainsCell(int x, int z, int axisX, int axisZ, float radius, float halfAngel)
        {
            double distanceSquared = (double)x * x + (double)z * z;
            if (distanceSquared == 0d || distanceSquared > (double)radius * radius) return false;

            double axisSquared = (double)axisX * axisX + (double)axisZ * axisZ;
            if (axisSquared == 0d) return false;

            double dot = (double)x * axisX + (double)z * axisZ;
            double threshold = Math.Sqrt(distanceSquared * axisSquared) * Math.Cos(halfAngel * Math.PI / 180d);
            return dot + 0.000001d >= threshold;
        }
    }
}
