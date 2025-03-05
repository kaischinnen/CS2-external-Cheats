using Newtonsoft.Json;

namespace CS2Cheats.Utils.DTO.ButtonsDTO;

public class ButtonsDTO
{
    [JsonProperty("client.dll")] public ClientDll clientdll { get; set; }
}
public class ClientDll
{
    public int attack { get; set; }
    public int attack2 { get; set; }
    public int back { get; set; }
    public int duck { get; set; }
    public int forward { get; set; }
    public int jump { get; set; }
    public int left { get; set; }
    public int lookatweapon { get; set; }
    public int reload { get; set; }
    public int right { get; set; }  
    public int showscores { get; set; }
    public int sprint { get; set; }
    public int turnleft { get; set; }
    public int turnright { get; set; }
    public int use { get; set; }
    public int zoom { get; set; }
}