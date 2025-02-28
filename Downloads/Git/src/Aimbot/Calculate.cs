using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public static class Calculate {
    public static Vector2 CalculateAngles(Vector3 from, Vector3 to) {
        float yaw, pitch;

        // calculate yaw <=> horizonal rotation
        float deltaX = to.X - from.X;
        float deltaY = to.Y - from.Y;
        yaw = (float)Math.Atan2(deltaY, deltaX) * (180 / MathF.PI); // convert to degrees

        // calculate pitch <=> vertical rotation
        float deltaZ = to.Z - from.Z;
        double distance = Math.Sqrt(Math.Pow(deltaX,2) + Math.Pow(deltaY, 2)); // pythagorean theorem
        pitch = -(float)Math.Atan2(deltaZ, distance) * 180 / MathF.PI; // deg

        return new Vector2(yaw, pitch);
    }
}