using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CS2Cheats
{
    class Cheats
    {
        // cancellation token sources for each task
        private static CancellationTokenSource _antiFlashCTS;
        private static CancellationTokenSource _radarHackCTS;
        private static CancellationTokenSource _bhopCTS;
        private static CancellationTokenSource _triggerbotCTS;

        static void Main()
        {
            // init Swed
            Swed swed = new Swed("cs2");

            // get client.dll base address
            IntPtr client = swed.GetModuleBase("client.dll");

            // init ImGui and overlay
            Renderer renderer = new Renderer();
            renderer.Start().Wait();
            Vector2 screenSize = renderer.screenSize;

            // entity handling
            List<Entity> entities = new List<Entity>();
            Entity localPlayer = new Entity();

            // consts
            const int HOTKEY = 0x06; // mouse 

            // main loop
            while (true)
            {
                // reset
                entities.Clear();
                Console.Clear();

                // get entity list
                IntPtr entityList = swed.ReadPointer(client + Offsets.dwEntityList);

                // first entry
                IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

                // get localPlayer information
                localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
                localPlayer.team = swed.ReadInt(localPlayer.pawnAddress + Offsets.m_iTeamNum);
                localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress + Offsets.m_vOldOrigin);
                localPlayer.view = swed.ReadVec(localPlayer.pawnAddress + Offsets.m_vecViewOffset);

                // initialization of other cheats

                // antiFlash
                // if antiFlash checkbox was turned on for the very first time, meaning the task is not running(cts is null) but the checkbox is on, so we wanna start the task (this is only being executed once).
                if (renderer.antiFlash && _antiFlashCTS == null)
                {
                    _antiFlashCTS = new CancellationTokenSource(); // create a new cts indicating status of task (running or not)
                    Task.Run(() => AntiFlash(swed, client, _antiFlashCTS.Token));
                    renderer.antiFlashRunning = 1; 
                }
                
                // if (checkbox is not checked and there is no task running) | (checkbox is checked and task is running) => We dont need to do anything
                if (!renderer.antiFlash && renderer.antiFlashRunning == 0 || renderer.antiFlash && renderer.antiFlashRunning == 1) { }

                // if the antiFlash checkbox was *just* turned off but the task is still running so we wanna cancel the task and toggle antiFlashRunning
                else if (!renderer.antiFlash && _antiFlashCTS != null)
                {
                    _antiFlashCTS.Cancel();
                    _antiFlashCTS.Dispose();
                    renderer.antiFlashRunning = 0;
                }

                // if the checkbox was turned on again after it being turned off, we have to cancel the old task, dispose the cts and start a new one
                else if (renderer.antiFlash && _antiFlashCTS != null)
                {   
                    renderer.antiFlashRunning = 1;
                    _antiFlashCTS = new CancellationTokenSource();
                    Task.Run(() => AntiFlash(swed, client, _antiFlashCTS.Token));
                }

                // radarHack
                // same logic as antiFlash. 
                if (renderer.radarHack && _radarHackCTS == null)
                {
                    _radarHackCTS = new CancellationTokenSource();
                    Task.Run(() => RadarHack(swed, client, _radarHackCTS.Token));
                    renderer.radarHackRunning = 1;
                }

                if (!renderer.radarHack && renderer.radarHackRunning == 0 || renderer.radarHack && renderer.radarHackRunning == 1) { }

                else if (!renderer.radarHack && _radarHackCTS != null)
                {
                    _radarHackCTS.Cancel();
                    _radarHackCTS.Dispose();
                    renderer.radarHackRunning = 0;
                }

                else if (renderer.radarHack && _radarHackCTS != null)
                {
                    renderer.radarHackRunning = 1;
                    _radarHackCTS = new CancellationTokenSource();
                    Task.Run(() => RadarHack(swed, client, _radarHackCTS.Token));
                }

                // bhop
                // you get it. 
                if (renderer.bhop && _bhopCTS == null)
                {
                    _bhopCTS = new CancellationTokenSource();
                    Task.Run(() => Bhop(swed, client, _bhopCTS.Token));
                    renderer.bhopRunning = 1;
                }

                if (!renderer.bhop && renderer.bhopRunning == 0 || renderer.bhop && renderer.bhopRunning == 1) { }

                else if (!renderer.bhop && _bhopCTS != null)
                {
                    _bhopCTS.Cancel();
                    _bhopCTS.Dispose();
                    renderer.bhopRunning = 0;
                }

                else if (renderer.bhop && _bhopCTS != null)
                {
                    renderer.bhopRunning = 1;
                    _bhopCTS = new CancellationTokenSource();
                    Task.Run(() => RadarHack(swed, client, _bhopCTS.Token));
                }

                // triggerbot
                if (renderer.triggerbot && _triggerbotCTS != null)
                {
                    _triggerbotCTS = new CancellationTokenSource();
                    Task.Run(() => TriggerBot(swed, client, _triggerbotCTS.Token));
                    renderer.triggerbotRunning = 1;
                }

                if (!renderer.triggerbot && renderer.triggerbotRunning == 0 || renderer.triggerbot && renderer.triggerbotRunning == 1) { }
                else if (!renderer.triggerbot && _triggerbotCTS != null)
                {
                    _triggerbotCTS.Cancel();
                    _triggerbotCTS.Dispose();
                    renderer.triggerbotRunning = 0;
                }
                else if (renderer.triggerbot && _triggerbotCTS != null)
                {
                    renderer.triggerbotRunning = 1;
                    _triggerbotCTS = new CancellationTokenSource();
                    Task.Run(() => TriggerBot(swed, client, _triggerbotCTS.Token));
                }

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
                        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF)); // bitmask: extracts index within the entry

                        if (currentPawn == localPlayer.pawnAddress) continue; // if the entity is us

                        // get scene node
                        IntPtr sceneNode = swed.ReadPointer(currentPawn + Offsets.m_pGameSceneNode);

                        // get bone array / bone matrix
                        IntPtr boneMatrix = swed.ReadPointer(sceneNode + Offsets.m_modelState + 0x80); // 0x80 is dwBoneMatrix offset

                        // get pawn attributes
                        int health = swed.ReadInt(currentPawn + Offsets.m_iHealth);
                        int team = swed.ReadInt(currentPawn + Offsets.m_iTeamNum);
                        uint lifestate = swed.ReadUInt(currentPawn + Offsets.m_lifeState);
                        string name = swed.ReadString(currentController + Offsets.m_iszPlayerName, 32);

                        // if attributes hold up, we add the entity to our own list
                        if (lifestate != 256) continue;
                        if (team == localPlayer.team && !renderer.aimOnTeam) continue;

                        Entity entity = new Entity();

                        // set entity attributes
                        entity.pawnAddress = currentPawn;
                        entity.controllerAddress = currentController;
                        entity.health = health;
                        entity.lifestate = lifestate;
                        entity.origin = swed.ReadVec(currentPawn + Offsets.m_vOldOrigin);
                        entity.view = swed.ReadVec(currentPawn + Offsets.m_vecViewOffset);
                        entity.distance = Vector3.Distance(localPlayer.origin, entity.origin);
                        entity.head = swed.ReadVec(boneMatrix, 6 * 32); // 6 = bone id for head, 32 = size of matrix = current step

                        // get 2d info
                        ViewMatrix viewMatrix = ReadMatrix(client + Offsets.dwViewMatrix);
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

                        Console.WriteLine($"{name}: {entity.health}hp, distance: {(int)entity.distance / 100}m, head coordinate: {entity.head}");

                        Console.ResetColor();
                    }

                // sort entities
                if (renderer.fovAimbot)
                {  // if fov aimbot is enabled, sort by pixel distance
                    entities = entities.OrderBy(o => o.pixelDistance).ToList();

                    // check if entity is in FOV
                    if (entities[0].pixelDistance > renderer.FOV) continue;
                }
                else
                {
                    entities = entities.OrderBy(o => o.distance).ToList(); // sort by distance
                }

                if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
                {    // count, hotkey, checkbox,

                    // get view pos
                    Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view); // origin is feet, view eye level
                    Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

                    // get angles
                    Vector2 newAngles = Calculate.CalculateAngles(playerView, entities[0].head);
                    Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f); // set y before x

                    // force new angles
                    swed.WriteVec(client + Offsets.dwViewAngles, newAnglesVec3);
                }
                Thread.Sleep(10);
            }

            // converting viewMatrix into our own matrix
            ViewMatrix ReadMatrix(IntPtr matrixAddr)
            {
                var viewMatrix = new ViewMatrix();
                var matrix = swed.ReadMatrix(matrixAddr);

                // converting into our matrix
                // there is probably a smarter way to do this but who doesn't love hardcode <<

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
        // AntiFlash 
        static void AntiFlash(Swed swed, IntPtr client, CancellationToken token)
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

        // Radarhack
        static void RadarHack(Swed swed, IntPtr client, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
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
                    IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF)); // bitmask: extracts index within the entry

                    // write over spotted status
                    swed.WriteBool(currentPawn + Offsets.m_entitySpottedState + Offsets.m_bSpotted, true);

                    Thread.Sleep(50);
                    Console.Clear();
                }
            }
        }

        // Bhop
        static void Bhop(Swed swed, IntPtr Client, CancellationToken token)
        {
            // consts
            const int SPACE_BAR = 0x20;

            const uint STANDING = 65665;
            const uint CROUCHING = 65667;

            // Jump and -jump
            const uint PLUS_JUMP = 65537;
            const uint MINUS_JUMP = 256;

            IntPtr client = swed.GetModuleBase("client.dll");
            IntPtr jumpAddress = client + Offsets.jump;
          
            while (!token.IsCancellationRequested)
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

        // Triggerbot
        static void TriggerBot(Swed swed, IntPtr client, CancellationToken token) {

            const int HOTKEY = 0x06; // mouse 4

            IntPtr attack = client + Offsets.attack;
            Entity localPlayer = new Entity();

            while (!token.IsCancellationRequested) {

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

                Console.WriteLine($"Target Team: {targetTeam}, LifeState: {targetLifeState}, Health: {targetHealth}");

                // if target is enemy and alive
                if (targetTeam == localPlayer.team && targetLifeState != 256)
                {
                    if (GetAsyncKeyState(HOTKEY) < 0)
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
}
