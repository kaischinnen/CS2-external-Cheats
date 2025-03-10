using System.Numerics;

namespace CS2Cheats.Utils;

public class Entity {
    public IntPtr pawnAddress { get; set; }
    public IntPtr controllerAddress { get; set; }
    public Vector3 origin { get; set; }
    public Vector3 view { get; set; }
    public Vector3 head { get; set; }
    public Vector2 head2d { get; set; } // represent where head is located on our screen
    public int health { get; set; }
    public int team { get; set; }
    public uint lifestate { get; set; }
    public float distance { get; set; } // from localPlayer
    public string name { get; set; } = string.Empty;
    public float pixelDistance { get; set; } // from crosshair
}