using Swed64;
using System.Net.WebSockets;
using System.Runtime.InteropServices;

Swed swed = new Swed("cs2");

IntPtr client = swed.GetModuleBase("client.dll");

IntPtr attack = client + 0x1882720; 

while (true) {
    Console.Clear();

    IntPtr localPlayerPawn = swed.ReadPointer(client, 0x1889F20); // dwLocalPlayerPawn
    int entIndex = swed.ReadInt(localPlayerPawn, 0x1458); // m_iIDEntIndex
    Console.WriteLine($"Crosshair/Entity ID: {entIndex}");

    if (GetAsyncKeyState(0x06) < 0 )  { // mouse 4
        if (entIndex > 0 ) {    // if *any* entity is in crosshair
            swed.WriteInt(attack, 65537); // +attack
            Thread.Sleep(1);
            swed.WriteInt(attack, 256); // -attack
        }
    }
    Thread.Sleep(1);
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey); // hotkey