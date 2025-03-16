using Swed64;
using System.Numerics;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

class RcsC
{
    // struct for Vector3 but with operator overloads
    struct mVector3
    {
        public float X, Y, Z;

        public mVector3(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        // operator overloads
        public static mVector3 operator +(mVector3 a, mVector3 b)
            => new mVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static mVector3 operator -(mVector3 a, mVector3 b)
            => new mVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static mVector3 operator *(mVector3 a, float b)
            => new mVector3(a.X * b, a.Y * b, a.Z * b);

        // implicit conversion to Vector3 so we can use it together with a Vector3 (which are some offsets)
        public static implicit operator Vector3(mVector3 v)
            => new Vector3(v.X, v.Y, v.Z);

        public static implicit operator mVector3(Vector3 v)
            => new mVector3(v.X, v.Y, v.Z);

        public static implicit operator mVector3(Vector2 v)
            => new mVector3(v.X, v.Y, 0);

    }

    public static void Rcs(Renderer renderer, Swed swed, IntPtr client, CancellationToken token)
    {
        Entity localPlayer = new Entity();

        // init oldPunch vector, which is 0,0,0 at the beginning
        mVector3 oldPunch = new Vector3(0, 0, 0);

        while (!token.IsCancellationRequested)
        {
            localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);

            // returns amount of shots have been fired (resets to 0 after some pause(if recoil resetted))
            int iShotsFired = swed.ReadInt(localPlayer.pawnAddress + Offsets.m_iShotsFired); 
            
            // aimpunch: how much has our view by the recoil
            mVector3 aimPunchAngle = swed.ReadVec(localPlayer.pawnAddress + Offsets.m_aimPunchAngle);

            // crosshair is always half way between where you start shooting and where you actually bullet lands, so we reverse this by multiplying aimPunchAngle by a factor 2
            mVector3 punchAngle = aimPunchAngle * 2;

            // read current view angles
            mVector3 viewAngles = swed.ReadVec(client + Offsets.dwViewAngles);


            if (iShotsFired > 1)
            {
                // calculate new angle
                mVector3 newAngle = viewAngles + oldPunch - punchAngle;

                Console.WriteLine(iShotsFired);

                // normalize angles
                newAngle.X = (Calculate.NormalizeAngles(newAngle.X, newAngle.Y)).X;
                newAngle.Y = (Calculate.NormalizeAngles(newAngle.X, newAngle.Y)).Y;

                // force new angle
                swed.WriteVec(client + Offsets.dwViewAngles, newAngle);
            }
            oldPunch = punchAngle;
        }
    }
}