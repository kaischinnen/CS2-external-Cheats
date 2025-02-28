﻿using Swed64;


// init Swed
Swed swed = new Swed("cs2");

// get client.dll base address
IntPtr client = swed.GetModuleBase("client.dll");

// offsets
int dwEntityList = 0x1A369E0;
int m_hPlayerPawn = 0x80C;
int m_bSpotted = 0x8;
int m_entitySpottedState = 0x23D0;

while (true)
{
    // get entity list
    IntPtr entityList = swed.ReadPointer(client + dwEntityList);

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
        int pawnHandle = swed.ReadInt(currentController + m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        // second entry, and now we get the specific pawn
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0xFFF) >> 9) + 0x10);
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF)); // bitmask: extracts index within the entry

        // write over spotted status
        swed.WriteBool(currentPawn + m_entitySpottedState + m_bSpotted, true);

        Thread.Sleep(50);
        Console.Clear();
    }
}