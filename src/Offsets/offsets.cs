// Credits to https://github.com/a2x/cs2-dumper

public static class Offsets
{
    // offsets.cs
    public static int dwViewAngles = 0x1AACA60;
    public static int dwLocalPlayerPawn = 0x188AF10;
    public static int dwEntityList = 0x1A369E0;
    public static int dwViewMatrix = 0x1AA27D0;

    // client.dll.cs
    public static int m_hPlayerPawn = 0x80C;
    public static int m_iHealth = 0x344;
    public static int m_vOldOrigin = 0x1324;
    public static int m_iTeamNum = 0x3E3;
    public static int m_vecViewOffset = 0xCB0;
    public static int m_lifeState = 0x348;
    public static int m_modelState = 0x170;
    public static int m_pGameSceneNode = 0x328;
    public static int m_iszPlayerName = 0x660;
    public static int m_flFlashBangTime = 0x13F8;
    public static int m_bSpotted = 0x8;
    public static int m_entitySpottedState = 0x23D0;
    public static int m_iIDEntIndex = 0x1458;

    // button.cs#
    public static int jump = 0x1883C20;
    public static int attack = 0x1883710;
}