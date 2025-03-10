using Swed64;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

class AntiFlashC
{
    public static void AntiFlash(Swed swed, IntPtr client, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            IntPtr localPlayerPawn = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);

            float flashDuration = swed.ReadFloat(localPlayerPawn, Offsets.m_flFlashBangTime);

            if (flashDuration > 0)
            {
                swed.WriteFloat(localPlayerPawn, Offsets.m_flFlashBangTime, 0);
                Console.WriteLine("Avoided Flash!");
            }
            Thread.Sleep(2); // check every 2 ms
        }
    }
}