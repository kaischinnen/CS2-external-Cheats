using System.Dynamic;
using Newtonsoft.Json;

using CS2Cheats.DTO.ClientDllDTO;
using CS2Cheats.Utils.DTO;
using CS2Cheats.Utils.DTO.ButtonsDTO;

namespace CS2Cheats.Utils;

public class Offsets 
{
    // offsets.cs
    public static int dwViewAngles;
    public static int dwLocalPlayerPawn;
    public static int dwEntityList;
    public static int dwViewMatrix;

    // client.dll.cs
    public static int m_hPlayerPawn;
    public static int m_iHealth;
    public static int m_vOldOrigin;
    public static int m_iTeamNum;
    public static int m_vecViewOffset;
    public static int m_lifeState;
    public static int m_modelState;
    public static int m_pGameSceneNode;
    public static int m_iszPlayerName;
    public static int m_flFlashBangTime;
    public static int m_bSpotted;
    public static int m_entitySpottedState;
    public static int m_iIDEntIndex;
    public static int m_iShotsFired;
    public static int m_aimPunchAngle;

    // button.cs
    public static int jump;
    public static int attack;

    public static async Task<string> FetchJson(string url)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    }

    public static async Task UpdateOffsets()
    {
        try
        {
            var sourceDataDw = JsonConvert.DeserializeObject<OffsetsDTO>(
                await FetchJson("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.json"));
            var sourceDataClient = JsonConvert.DeserializeObject<ClientDllDTO>(
                await FetchJson("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.json"));
            var sourceDataButtons = JsonConvert.DeserializeObject<ButtonsDTO>(
              await FetchJson("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.json"));

            dynamic destData = new ExpandoObject();

            // Offsets

            // offsets
            destData.dwViewAngles = sourceDataDw.clientdll.dwViewAngles;
            destData.dwLocalPlayerPawn = sourceDataDw.clientdll.dwLocalPlayerPawn;
            destData.dwEntityList = sourceDataDw.clientdll.dwEntityList;
            destData.dwViewMatrix = sourceDataDw.clientdll.dwViewMatrix;

            // client
            destData.m_hPlayerPawn = sourceDataClient.clientdll.classes.CBasePlayerController.fields.m_hPawn;
            destData.iHealth = sourceDataClient.clientdll.classes.C_BaseEntity.fields.m_iHealth;
            destData.m_vOldOrigin = sourceDataClient.clientdll.classes.C_BasePlayerPawn.fields.m_vOldOrigin;
            destData.m_iTeamNum = sourceDataClient.clientdll.classes.C_BaseEntity.fields.m_iTeamNum;
            destData.m_vecViewOffset = sourceDataClient.clientdll.classes.C_BaseModelEntity.fields.m_vecViewOffset;
            destData.m_lifeState = sourceDataClient.clientdll.classes.C_BaseEntity.fields.m_lifeState;
            destData.m_modelState = sourceDataClient.clientdll.classes.CSkeletonInstance.fields.m_modelState;
            destData.m_pGameSceneNode = sourceDataClient.clientdll.classes.C_BaseEntity.fields.m_pGameSceneNode;
            destData.m_iszPlayerName = sourceDataClient.clientdll.classes.CBasePlayerController.fields.m_iszPlayerName;
            destData.m_flFlashBangTime = sourceDataClient.clientdll.classes.C_CSPlayerPawnBase.fields.m_flFlashBangTime;
            destData.m_bSpotted = sourceDataClient.clientdll.classes.EntitySpottedState_t.fields.m_bSpotted;
            destData.m_entitySpottedState = sourceDataClient.clientdll.classes.C_CSPlayerPawn.fields.m_entitySpottedState;
            destData.m_iIDEntIndex = sourceDataClient.clientdll.classes.C_CSPlayerPawnBase.fields.m_iIDEntIndex;
            destData.m_iShotsFired = sourceDataClient.clientdll.classes.C_CSPlayerPawn.fields.m_iShotsFired;
            destData.m_aimPunchAngle = sourceDataClient.clientdll.classes.C_CSPlayerPawn.fields.m_aimPunchAngle;


            // buttons
            destData.jump = sourceDataButtons.clientdll.jump;
            destData.attack = sourceDataButtons.clientdll.attack;

            UpdateStaticFields(destData);
        } catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
            throw;
        }
    }
    private static void UpdateStaticFields(dynamic data)
    {
        dwViewAngles = data.dwViewAngles;
        dwLocalPlayerPawn = data.dwLocalPlayerPawn;

        dwEntityList = data.dwEntityList;
        dwViewMatrix = data.dwViewMatrix;
        m_hPlayerPawn = data.m_hPlayerPawn;
        m_iHealth = data.iHealth;
        m_vOldOrigin = data.m_vOldOrigin;
        m_iTeamNum = data.m_iTeamNum;
        m_vecViewOffset = data.m_vecViewOffset;
        m_lifeState = data.m_lifeState;
        m_modelState = data.m_modelState;
        m_pGameSceneNode = data.m_pGameSceneNode;
        m_iszPlayerName = data.m_iszPlayerName;
        m_flFlashBangTime = data.m_flFlashBangTime;
        m_bSpotted = data.m_bSpotted;
        m_entitySpottedState = data.m_entitySpottedState;
        m_iIDEntIndex = data.m_iIDEntIndex;
        m_iShotsFired = data.m_iShotsFired;
        m_aimPunchAngle = data.m_aimPunchAngle;

        jump = data.jump;
        attack = data.attack;;   
    }
}