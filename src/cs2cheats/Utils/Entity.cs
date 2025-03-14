using System.Numerics;

namespace CS2Cheats.Utils;

public class Entity {
    public IntPtr pawnAddress { get; set; }
    public IntPtr controllerAddress { get; set; }
    public Vector3 origin { get; set; }
    public Vector2 position2d { get; set; } // represent where entity is located on our screen
    public Vector3 view { get; set; }
    public Vector2 viewPosition2d { get; set; } // represent where entity's view is located on our screen
    public Vector3 head { get; set; }
    public Vector2 head2d { get; set; } // represent where head is located on our screen
    public int health { get; set; }
    public int team { get; set; }
    public int lifestate { get; set; }
    public float distance { get; set; } // from localPlayer
    public string name { get; set; } = string.Empty;
    public float pixelDistance { get; set; } // from crosshair
    public List<Vector3>? bones { get; set; } 
    public List<Vector2>? bones2d { get; set; }}

public enum BoneIds
{
    Waist = 0, // 0
    Neck = 5, // 1
    Head = 6, // 2
    ShoulderLeft = 8, // 3
    ForeLeft = 9, // 4
    HandLeft = 11, // 5
    ShoulderRight = 13, // 6
    ForeRight = 14, // 7
    HandRight = 16, // 8   
    KneeLeft = 23, // 9
    FeetLeft = 24, // 10
    KneeRight = 26, // 11
    FeetRight = 27 // 12
}