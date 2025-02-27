﻿using Swed64;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace TriggerBot
{
    public class Entity
    {
        public IntPtr pawnAddress { get; set; }
        public int team { get; set; }
        public uint m_lifeState { get; set; }
    }

    class Program
    {
        const int HOTKEY = 0x06; // Mouse 4
        static Swed swed = new Swed("cs2");
        static IntPtr client = swed.GetModuleBase("client.dll");
        static IntPtr attack = client + 0x1882720;

        static int dwLocalPlayerPawn = 0x1889F20;
        static int dwEntityList = 0x1A359B0;
        static int m_iTeamNum = 0x3E3;
        static int m_lifeState = 0x348;
        static int m_iIDEntIndex = 0x1458;
        static int m_iHealth = 0x344;
        static void Main()
        {
            Entity localPlayer = new Entity();
            
            while (true)
            {
                Console.Clear();
                
                // get local player
                localPlayer.pawnAddress = swed.ReadPointer(client, dwLocalPlayerPawn);
                if (localPlayer.pawnAddress == IntPtr.Zero)
                {
                    Thread.Sleep(10);
                    continue;
                }
                
                // get local player team
                localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, m_iTeamNum);
                Console.WriteLine($"LocalPlayer Team: {localPlayer.team}");

                // get crosshair entity
                int targetEntityIndex = swed.ReadInt(localPlayer.pawnAddress, m_iIDEntIndex);
                Console.WriteLine($"Crosshair Entity Index: {targetEntityIndex}");
                
                // if crosshair is on no entity
                if (targetEntityIndex <= 0) {
                    Console.WriteLine("No entity found");
                    Thread.Sleep(5);
                    continue;
                }

                // accessing entity list
                IntPtr entityList = swed.ReadPointer(client, dwEntityList);
                IntPtr listEntry = swed.ReadPointer(entityList, 0x8 * (targetEntityIndex >> 9) + 0x10); // apply bitmask 0x7FFF and shift bits by 9
                IntPtr targetEntity = swed.ReadPointer(listEntry, 0x78 * (targetEntityIndex & 0x1FF));  // read current pawn, apply bitmask to stay in range
                
                // if target entry is invalid or local player
                if (targetEntity == IntPtr.Zero || targetEntity == localPlayer.pawnAddress) {
                    Thread.Sleep(10);
                    continue;
                }
                
                // get attributes
                int targetTeam = swed.ReadInt(targetEntity, m_iTeamNum);
                uint targetLifeState = swed.ReadUInt(targetEntity, m_lifeState);
                int targetHealth = swed.ReadInt(targetEntity, m_iHealth);

                Console.WriteLine($"Target Team: {targetTeam}, LifeState: {targetLifeState}, Health: {targetHealth}");
                
                // if target is enemy and alive
                if (targetTeam != localPlayer.team && targetLifeState == 256)
                {
                    if (GetAsyncKeyState(HOTKEY) < 0)
                    {
                        swed.WriteInt(attack, 65537); // +attack
                        Thread.Sleep(1);
                        swed.WriteInt(attack, 256);   // -attack
                    }
                }
                Thread.Sleep(1);
            }
        }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}
