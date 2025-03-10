using Swed64;
using System.Runtime.InteropServices;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

class TriggerbotC
{
    public static void Triggerbot(Renderer renderer, Swed swed, IntPtr client, CancellationToken token)
    {
        IntPtr attack = client + Offsets.attack;
        Entity localPlayer = new Entity();

        while (!token.IsCancellationRequested)
        {

            localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);

            if (localPlayer.pawnAddress == IntPtr.Zero)
            {
                Thread.Sleep(10);
                continue;
            }

            localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
            int targetEntityIndex = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iIDEntIndex);
            if (targetEntityIndex <= 0)
            {
                Thread.Sleep(5);
                continue;
            }

            IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);
            IntPtr listEntry = swed.ReadPointer(entityList, 0x8 * (targetEntityIndex >> 9) + 0x10);
            IntPtr targetEntity = swed.ReadPointer(listEntry, 0x78 * (targetEntityIndex & 0x1FF));

            if (targetEntity == IntPtr.Zero || targetEntity == localPlayer.pawnAddress)
            {
                Thread.Sleep(10);
                continue;
            }

            // get attributes
            int targetTeam = swed.ReadInt(targetEntity, Offsets.m_iTeamNum);
            uint targetLifeState = swed.ReadUInt(targetEntity, Offsets.m_lifeState);
            int targetHealth = swed.ReadInt(targetEntity, Offsets.m_iHealth);

            // if target is enemy and alive
            if (targetTeam == localPlayer.team && targetLifeState != 256)
            {
                if (GetAsyncKeyState(renderer.hotkey) < 0)
                {
                    swed.WriteInt(attack, 65537); // +attack
                    Thread.Sleep(1);
                    swed.WriteInt(attack, 256); // -attack
                }
            }
            Thread.Sleep(1);
            }
        }

    // hotkey import
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);
}