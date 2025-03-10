using Swed64;
using System.Runtime.InteropServices;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

class BhopC
{
    public static void Bhop(Swed swed, IntPtr client, CancellationToken token)
    {
        // consts
        const int SPACE_BAR = 0x20;

        const uint STANDING = 65665;
        const uint CROUCHING = 65667;

        // jump and -jump
        const uint PLUS_JUMP = 65537;
        const uint MINUS_JUMP = 256;

        IntPtr jumpAddress = client + Offsets.jump;

        while (!token.IsCancellationRequested)
        {
            IntPtr playerPawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
            uint fFlag = swed.ReadUInt(playerPawnAddress, 0x3EC);

            if (GetAsyncKeyState(SPACE_BAR) < 0)
            {
                if (fFlag == STANDING || fFlag == CROUCHING) // If grounded
                {
                    Thread.Sleep(1);
                    swed.WriteUInt(jumpAddress, PLUS_JUMP); // +jump 
                }
                else
                {
                    swed.WriteUInt(jumpAddress, MINUS_JUMP); // -jump
                }
            }
            Thread.Sleep(10);
        }
    }

    // hotkey import
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);
}
