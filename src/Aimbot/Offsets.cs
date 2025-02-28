using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text;
using System.Threading.Tasks;

public static class Offsets {
    // offsets.cs
    public static int dwViewAngles = 0x1AACA60;
    public static int dwLocalPlayerPawn = 0x188AF10;
    public static int dwEntityList = 0x1A369E0;

    // client.dll.cs
    public static int m_hPlayerPawn = 0x80C;
    public static int m_iHealth = 0x344;
    public static int m_vOldOrigin = 0x1324;
    public static int m_iTeamNum = 0x3E3;
    public static int m_vecViewOffset = 0xCB0;
    public static int m_lifeState = 0x348;
    public static int m_modelState = 0x170; 
    public static int m_pGameSceneNode = 0x328; 
    public static int m_iszPlayerName = 0x660;}