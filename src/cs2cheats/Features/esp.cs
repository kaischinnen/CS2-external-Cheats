using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

// for detailed comments, check aimbot.cs (identical code, up until the actual esp part)
class EspC
{
    public static void Esp(Renderer renderer, Swed swed, IntPtr client, CancellationToken token)
    {
        Reader reader = new Reader(swed);

        List<Entity> entities = new List<Entity>();
        Entity localPlayer = new Entity();
;

        while (!token.IsCancellationRequested)
        {
            entities.Clear();
            Console.Clear();

            IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

            IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

            localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
            localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
            localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);

            for (int i = 0; i < 64; i++)
            {
                if (listEntry == IntPtr.Zero) continue;

                IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); 

                if (currentController == IntPtr.Zero) continue;

                int pawnHandle = swed.ReadInt(currentController + Offsets.m_hPlayerPawn);
                if (pawnHandle == 0) continue;

                IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0xFFF) >> 9) + 0x10);
                IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF)); 

                if (currentPawn == localPlayer.pawnAddress) continue;

                IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);
                IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);

                ViewMatrix viewMatrix = Reader.ReadMatrix(swed, client + Offsets.dwViewMatrix);

                int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);   
                int lifeState = swed.ReadInt(currentPawn, Offsets.m_lifeState);

                if (lifeState != 256) continue;

                Entity entity = new Entity();

                entity.pawnAddress = currentPawn;
                entity.team = team;
                entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
                entity.controllerAddress = currentController;
                entity.lifestate = lifeState;
                entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);
                entity.bones = reader.ReadBones(boneMatrix);
                entity.bones2d = reader.ReadBones2d(entity.bones, viewMatrix, renderer.screenSize);

                entities.Add(entity);

                Console.ForegroundColor = ConsoleColor.Green;

                if (team != localPlayer.team)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.ResetColor();

            }

            // fetch over to renderer
            renderer.entitiesCopy = entities;
            renderer.localPlayerCopy = localPlayer;
            Thread.Sleep(3);
        }
    }
}