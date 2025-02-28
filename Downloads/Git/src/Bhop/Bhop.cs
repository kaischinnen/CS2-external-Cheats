using System;
using System.Threading;
using System.Runtime.InteropServices;
using Swed64;

class Bhop
{
    static void Main()
    {   
        Swed swed = new Swed("cs2");

        // Hotkey
        const int SPACE_BAR = 0x20;

        const uint STANDING = 65665;
        const uint CROUCHING = 65667;

        // Jump and -jump
        const uint PLUS_JUMP = 65537;
        const uint MINUS_JUMP = 256;

        IntPtr client = swed.GetModuleBase("client.dll");
        IntPtr jumpAddress = client + 0x1883C20;

        while (true)
        {
            IntPtr playerPawnAddress = swed.ReadPointer(client, 0x188AF10);
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
            Thread.Sleep(5);
        }
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}