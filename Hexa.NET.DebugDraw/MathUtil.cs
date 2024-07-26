namespace Hexa.NET.DebugDraw
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    internal static class MathUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 BillboardLH(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
        {
            Vector3 crossed;
            Vector3 final;
            Vector3 difference = cameraPosition - objectPosition;

            float lengthSq = difference.LengthSquared();
            if (lengthSq == 0)
            {
                difference = -cameraForwardVector;
            }
            else
            {
                difference *= (float)(1.0 / Math.Sqrt(lengthSq));
            }

            crossed = Vector3.Cross(cameraUpVector, difference);
            crossed = Vector3.Normalize(crossed);
            final = Vector3.Cross(difference, crossed);

            Matrix4x4 result = new();
            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;

            return result;
        }
    }
}
