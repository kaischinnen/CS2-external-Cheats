using Swed64;
using System.Numerics;
using CS2Cheats.Utils;

namespace CS2Cheats.Features;

// for detailed comments, check aimbot.cs (identical code, up until the actual esp part)
class EspC
{
    public static void Esp(Renderer renderer, Swed swed, IntPtr client, CancellationToken token)
    {
        Vector2 screenSize = renderer.screenSize;

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

                // get current weapon
                IntPtr currentWeapon = swed.ReadPointer(currentPawn, Offsets.m_pClippingWeapon);

                short weaponDefinitionIndex = swed.ReadShort(currentWeapon, Offsets.m_AttributeManager + Offsets.m_Item + Offsets.m_iItemDefinitionIndex);
                if (weaponDefinitionIndex == -1) continue;

                IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);
                IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);

                int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);   
                int lifeState = swed.ReadInt(currentPawn, Offsets.m_lifeState);
                string name = swed.ReadString(currentController, Offsets.m_iszPlayerName, 32);

                if (lifeState != 256) continue;

                ViewMatrix viewMatrix = Reader.ReadMatrix(swed, client + Offsets.dwViewMatrix);

                Entity entity = new Entity();

                // update attributes
                entity.health = swed.ReadInt(currentPawn, Offsets.m_iHealth); // health
                entity.team = team; // team
                entity.lifestate = lifeState; // lifestate
                entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin); // origin position on map (3d)
                entity.position2d = Calculate.WorldToScreen(viewMatrix, entity.origin, (int)renderer.screenSize.X, (int)renderer.screenSize.Y); // 2d 
                entity.distance = Vector3.Distance(entity.origin, localPlayer.origin); // distance from entity to localPlayer
                entity.bones = Calculate.ReadBones(boneMatrix, swed); // list of Vec3 bones
                entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, renderer.screenSize); // bones on screen
                entity.view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset); // view position
                entity.viewPosition2d = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.origin, entity.view), (int)renderer.screenSize.X, (int)renderer.screenSize.Y); // view position on screen
                entity.viewMatrix = viewMatrix; // view matrix
                entity.head = swed.ReadVec(boneMatrix, 6 * 32); // head position
                entity.head2d = Calculate.WorldToScreen(viewMatrix, entity.head, (int)renderer.screenSize.X, (int)renderer.screenSize.Y); // head position on screen
                entity.pixelDistance = Vector2.Distance(entity.head2d, new Vector2(screenSize.X / 2, screenSize.Y / 2));
                entity.name = name;
                entity.currentWeaponIndex = weaponDefinitionIndex;
                entity.currentWeaponName = Enum.GetName(typeof(Weapons), weaponDefinitionIndex);

                entities.Add(entity);

                Console.ForegroundColor = ConsoleColor.Green;

                if (team != localPlayer.team)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.ResetColor();

            }

            entities = entities.OrderBy(o => o.pixelDistance).ToList();
            try
            {
                float temp = entities[0].pixelDistance;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("There seems to be no entitites in this server!");
                Thread.Sleep(200);
                continue;
            }
;
            // fetch over to renderer
            renderer.UpdateLocalPlayer(localPlayer);
            renderer.UpdateEntities(entities);
            renderer.UpdateClosestPlayer(entities[0]);

            Thread.Sleep(1);
        }
    }
}