using System.Numerics;

public class Entity {
    public IntPtr pawnAddress { get; set; }
    public IntPtr controllerAddress { get; set; }
    public Vector3 origin { get; set; }
    public Vector3 view { get; set; }
    public Vector3 head { get; set; }
    public int health { get; set; }
    public int team { get; set; }
    public uint lifestate { get; set; }
    public float distance { get; set; } // from localPlayer
    public string name { get; set; } = string.Empty;
}