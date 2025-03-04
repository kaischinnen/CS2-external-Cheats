using System.Numerics;

public static class Calculate
{
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
    public static Vector2 WorldToScreen(ViewMatrix matrix, Vector3 pos, int width, int height) {
        Vector2 screenCoordinates = new Vector2();

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
            float X = camX + (screenX / screenWidth) * camX;
            float Y = camY - (screenY / screenWidth) * camY;

            // return screen coords
            screenCoordinates = new Vector2(X, Y);
            return screenCoordinates;
        }
        else
        {
            return new Vector2(-1, -1);
        }
    }
}