using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

// init Swed
Swed swed = new Swed("cs2");

// get client.dll base address
IntPtr client = swed.GetModuleBase("client.dll");

// init ImGui and overlay
Renderer renderer = new Renderer();
renderer.Start().Wait(); 

// entity handling
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

// consts
const int HOTKEY = 0x06; // mouse 5

// main loop
while (true) {

    //reset
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

    // loop through entity list

    for (int i = 0; i < 64; i++) { // max 64 entities

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

        // if aatributes hold up, we add the entity to our own list
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

        // addign entity to our own entity
        entities.Add(entity);

        // draw to console
        Console.ForegroundColor = ConsoleColor.Green;

        if (team != localPlayer.team) {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine($"{entity.health}hp, distance: {(int)entity.distance / 100}m, head coordinate: {entity.head}");
        
        Console.ResetColor();
    }

    // sort entities and aim
    entities = entities.OrderBy(o => o.distance).ToList(); // closest

    if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot) {    // count, hotkey, checkbox,

        // get view pos
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view); // origin is feet, view eye level
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

        // get angles
        Vector2 newAngles = Calculate.CalculateAngles(playerView, entities[0].head);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f); // set y before x

        // force new angles
        swed.WriteVec(client + Offsets.dwViewAngles, newAnglesVec3);
    }
    Thread.Sleep(20); 
}

// hotkey import
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);