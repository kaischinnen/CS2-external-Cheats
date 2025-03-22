using Swed64;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

class GlowC
{
    public static void Glow(Renderer renderer, Swed swed, IntPtr client, CancellationToken token)
    {
        List<Entity> entities = new List<Entity>();
        Entity localPlayer = new Entity();

        while (!token.IsCancellationRequested)
        {
            entities.Clear();
            Console.Clear();

            localPlayer.team = swed.ReadInt(localPlayer.pawnAddress + Offsets.m_iTeamNum);
            localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);

            // get entity list
            IntPtr entityList = swed.ReadPointer(client + Offsets.dwEntityList);

            // first entry
            IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

            // loop through entity list
            for (int i = 0; i < 64; i++) // max 64 entities
            {
                if (listEntry == IntPtr.Zero) continue;

                // get current controller
                IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); // step = 0x78
                if (currentController == IntPtr.Zero) continue;

                // get pawn
                int pawnHandle = swed.ReadInt(currentController + Offsets.m_hPlayerPawn);
                if (pawnHandle == 0) continue;

                // second entry, and now we get the specific pawn
                IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0xFFF) >> 9) + 0x10);
                IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF)); // bitmask: extracts index within the entrys

                if (currentPawn == localPlayer.pawnAddress) continue;

                int team = swed.ReadInt(currentPawn + Offsets.m_iTeamNum);
                int lifestate = swed.ReadInt(currentPawn + Offsets.m_lifeState);

                if (lifestate != 256) continue;

                if (!renderer.glowTeam && team == localPlayer.team) continue;

                Entity entity = new Entity();

                entity.team = team;
                entity.lifestate = lifestate;

                entities.Add(entity);

                // if we only glow enemies
                if (!renderer.glowTeam)
                {
                    swed.WriteLong(currentPawn + Offsets.m_Glow + Offsets.m_glowColorOverride, renderer.glow64);
                    swed.WriteInt(currentPawn + Offsets.m_Glow + Offsets.m_bGlowing, 1);
                }

                else
                {
                    // glow team color
                    if (localPlayer.team == team)
                    {
                        swed.WriteLong(currentPawn + Offsets.m_Glow + Offsets.m_glowColorOverride, renderer.glowTeam64);
                        swed.WriteInt(currentPawn + Offsets.m_Glow + Offsets.m_bGlowing, 1);
                    }

                    else
                    {
                        // glow enemy color
                        swed.WriteLong(currentPawn + Offsets.m_Glow + Offsets.m_glowColorOverride, renderer.glowEnemy64);
                        swed.WriteInt(currentPawn + Offsets.m_Glow + Offsets.m_bGlowing, 1);
                    }
                }
            }
        }
    }
}
