﻿using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;


namespace CS2Cheats.Features;

class AimbotC
{
    public static void Aimbot(Renderer renderer, Swed swed, IntPtr client, CancellationToken token)
    {
        Vector2 screenSize = renderer.screenSize;

        // entity handling
        List<Entity> entities = new List<Entity>();
        Entity localPlayer = new Entity();

        // consts
        const int HOTKEY = 0x06; // mouse 4

        while (!token.IsCancellationRequested)
        {
            // reset
            entities.Clear();
            Console.Clear();

            // get entity list
            IntPtr entityList = swed.ReadPointer(client + Utils.Offsets.dwEntityList);

            // first entry
            IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

            // get localPlayer information
            localPlayer.pawnAddress = swed.ReadPointer(client, Utils.Offsets.dwLocalPlayerPawn);
            localPlayer.team = swed.ReadInt(localPlayer.pawnAddress + Utils.Offsets.m_iTeamNum);
            localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress + Utils.Offsets.m_vOldOrigin);
            localPlayer.view = swed.ReadVec(localPlayer.pawnAddress + Utils.Offsets.m_vecViewOffset);

            // loop through entity list
            for (int i = 0; i < 64; i++) // max 64 entities
            {
                if (listEntry == IntPtr.Zero) continue;

                // get current controller
                IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); // step = 0x78

                if (currentController == IntPtr.Zero) continue;

                // get pawn
                int pawnHandle = swed.ReadInt(currentController + Utils.Offsets.m_hPlayerPawn);
                if (pawnHandle == 0) continue;

                // second entry, and now we get the specific pawn
                IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0xFFF) >> 9) + 0x10);
                IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF)); // bitmask: extracts index within the entry

                if (currentPawn == localPlayer.pawnAddress) continue; // if the entity is us

                // get scene node
                IntPtr sceneNode = swed.ReadPointer(currentPawn + Utils.Offsets.m_pGameSceneNode);

                // get bone array / bone matrix
                IntPtr boneMatrix = swed.ReadPointer(sceneNode + Utils.Offsets.m_modelState + 0x80); // 0x80 is dwBoneMatrix offset

                // get pawn attributes
                int health = swed.ReadInt(currentPawn + Utils.Offsets.m_iHealth);
                int team = swed.ReadInt(currentPawn + Utils.Offsets.m_iTeamNum);
                uint lifestate = swed.ReadUInt(currentPawn + Utils.Offsets.m_lifeState);
                string name = swed.ReadString(currentController + Utils.Offsets.m_iszPlayerName, 32);

                // if attributes hold up, we add the entity to our own list
                if (lifestate != 256) continue;
                if (team == localPlayer.team && !renderer.aimOnTeam) continue;

                Entity entity = new Entity();

                // set entity attributes
                entity.pawnAddress = currentPawn;
                entity.controllerAddress = currentController;
                entity.health = health;
                entity.lifestate = lifestate;
                entity.origin = swed.ReadVec(currentPawn + Utils.Offsets.m_vOldOrigin);
                entity.view = swed.ReadVec(currentPawn + Utils.Offsets.m_vecViewOffset);
                entity.distance = Vector3.Distance(localPlayer.origin, entity.origin);
                entity.head = swed.ReadVec(boneMatrix, 6 * 32); // 6 = bone id for head, 32 = size of matrix = current step

                // get 2d info
                ViewMatrix viewMatrix = ReadMatrix(swed, client + Utils.Offsets.dwViewMatrix);
                //get head
                entity.head2d = Calculate.WorldToScreen(viewMatrix, entity.head, (int)screenSize.X, (int)screenSize.Y); // floats have to be converted to int
                                                                                                                        // get distance from crosshair
                entity.pixelDistance = Vector2.Distance(entity.head2d, new Vector2(screenSize.X / 2, screenSize.Y / 2));

                // adding entity to our own entity list
                entities.Add(entity);

                // draw to console
                Console.ForegroundColor = ConsoleColor.Green;

                if (team != localPlayer.team)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                // print coords only if aimbot or fov aimbot is running
                if (renderer.fovAimbotRunning == 1 | renderer.aimbotRunning == 1)
                {
                    Console.WriteLine($"{name}: {entity.health}hp, distance: {(int)entity.distance / 100}m, head coordinate: {entity.head}");
                    Console.ResetColor();
                }
            }

            // sort entities
            // if fov aimbot is turned on
            if (renderer.aimbotMode == 1)
            {  
                // if fov aimbot is enabled, sort entities by pixel distance
                entities = entities.OrderBy(o => o.pixelDistance).ToList();

                // check if entity is in FOV
                // if player hasnt joined any server or there are no entities existing
                try
                {
                    float temp = entities[0].pixelDistance;
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("There seems to be no entitites in this server!");
                    Thread.Sleep(50);
                    continue;
                }
                if (entities[0].pixelDistance > renderer.FOV)
                {
                    Thread.Sleep(2);
                    continue;
                }
            }

            // if fov aimbot is disabled, we sort by distance
            else
            {
                entities = entities.OrderBy(o => o.distance).ToList(); // sort by distance
            }

            if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && (renderer.aimbot | renderer.fovAimbot))
            {    // count, hotkey, checkbox,

                // get view pos
                Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view); // origin is feet, view eye level
                Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

                // get angles
                Vector2 newAngles = Calculate.CalculateAngles(playerView, entities[0].head);
                Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f); // set y before x

                // force new angles
                swed.WriteVec(client + Utils.Offsets.dwViewAngles, newAnglesVec3);
            }
        }

        // converting viewMatrix into our own matrix
        static ViewMatrix ReadMatrix(Swed swed, IntPtr matrixAddr)
        {
            var viewMatrix = new ViewMatrix();
            var matrix = swed.ReadMatrix(matrixAddr);

            // converting into our matrix
            // there is probably a smarter way to do this but who doesn't love hardcodíng <<

            // first row
            viewMatrix.m11 = matrix[0];
            viewMatrix.m12 = matrix[1];
            viewMatrix.m13 = matrix[2];
            viewMatrix.m14 = matrix[3];

            // second row
            viewMatrix.m21 = matrix[4];
            viewMatrix.m22 = matrix[5];
            viewMatrix.m23 = matrix[6];
            viewMatrix.m24 = matrix[7];

            // third row
            viewMatrix.m31 = matrix[8];
            viewMatrix.m32 = matrix[9];
            viewMatrix.m33 = matrix[10];
            viewMatrix.m34 = matrix[11];

            // fourth row
            viewMatrix.m41 = matrix[12];
            viewMatrix.m42 = matrix[13];
            viewMatrix.m43 = matrix[14];
            viewMatrix.m44 = matrix[15];

            return viewMatrix;
        }
    }

    // hotkey import
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vkey);
}