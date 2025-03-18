using Swed64;
using System.Numerics;

namespace CS2Cheats.Utils;

public static class Calculate
{
    // calc angle between two vectors
    public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
    {
        float yaw, pitch;

        // calculate yaw <=> horizonal rotation
        float deltaX = to.X - from.X;
        float deltaY = to.Y - from.Y;
        yaw = (float)Math.Atan2(deltaY, deltaX) * (180 / MathF.PI); // convert to degrees

        // calculate pitch <=> vertical rotation
        float deltaZ = to.Z - from.Z;
        double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)); // pythagorean theorem
        pitch = -(float)Math.Atan2(deltaZ, distance) * 180 / MathF.PI; // deg

        return new Vector2(yaw, pitch);
    }

    // w2s
    public static Vector2 WorldToScreen(ViewMatrix matrix, Vector3 pos, int width, int height) 
    {
        // get screenWidth
        float screenWidth = (matrix.m41 * pos.X) + (matrix.m42 * pos.Y) + (matrix.m43 * pos.Z + matrix.m44);

        if (screenWidth > 0.001f)
        {
            // calc screen X and Y
            float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z + matrix.m14);
            float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z + matrix.m24);

            // camera center
            float camX = width / 2;
            float camY = height / 2;

            // perspective division
            float X = camX + camX * screenX / screenWidth;
            float Y = camY - camY * screenY / screenWidth;

            // return screen coords
            return new Vector2(X, Y);
        }
        else
        {
            return new Vector2(-99, -99);
        }
    }

    // lineare interpolation between two values (start, end) with value t being in range [0, 1]
    public static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    // read bones from entity
    public static List<Vector3> ReadBones(IntPtr boneAddress, Swed swed)
    {
        byte[] boneBytes = swed.ReadBytes(boneAddress, 27 * 32 + 16); // get max, 27 = id, 32 = step, 16 = size of matrix
        List<Vector3> bones = new List<Vector3>();

        // loop through all the bones 
        foreach (var boneId in Enum.GetValues(typeof(BoneIds)))
        {
            // convert bytes to float
            float x = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 0); // float = 4 bytes
            float y = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 4);
            float z = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 8);

            // add bone to list
            Vector3 currentBone = new Vector3(x, y, z);
            bones.Add(currentBone);
        }
        return bones;
    }

    // read bones from entity and convert them to 2d
    public static List<Vector2> ReadBones2d(List<Vector3> bones, ViewMatrix viewMatrix, Vector2 screenSize)
    {
        List<Vector2> bones2d = new List<Vector2>();
        foreach (Vector3 bone in bones)
        {
            Vector2 bone2d = WorldToScreen(viewMatrix, bone, (int)screenSize.X, (int)screenSize.Y);
            bones2d.Add(bone2d);
        }
        return bones2d;
    }

    public static Vector3 NormalizeAngles(Vector3 angles)
    {
        while (angles.Y < -180) angles.Y += 360;
        while (angles.Y > 180) angles.Y -= 360;
        if (angles.X > 89) angles.X = 89;
        if (angles.X < -89) angles.X = -89;

        return angles;
    }   

}