using Swed64;
using System.Numerics;

namespace CS2Cheats.Utils;
public class Reader
{
    // init swed
    Swed swed;

    // constructor
    public Reader(Swed swed) 
    { 
        this.swed = swed; }

    // read bones from entity
    public List<Vector3> ReadBones(IntPtr boneAddress)
    {
        byte[] boneBytes = swed.ReadBytes(boneAddress, 27 * 32 + 16); // get max, 27 = id, 32 = step, 16 = size of matrix
        List<Vector3> bones = new List<Vector3>();

        // loop through all the bones 
        foreach (var boneId in Enum.GetValues(typeof(BoneIds)))
        {   
            float x = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 0); // float = 4 bytes
            float y = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 4);
            float z = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 8);

            // add bone to list
            Vector3 currentBone = new Vector3(x, y, z);
            bones.Add(currentBone);
        }
        return bones;
    }

    // read bones from entity and convert them to 2d
    public List<Vector2> ReadBones2d(List<Vector3> bones, ViewMatrix viewMatrix, Vector2 screenSize)
    {
        List<Vector2> bones2d = new List<Vector2>();
        foreach (Vector3 bone in bones)
        {
            Vector2 bone2d = Calculate.WorldToScreen(viewMatrix, bone, (int)screenSize.X, (int)screenSize.Y);
            bones2d.Add(bone2d);
        }
        return bones2d;
    }

    // converting viewMatrix into our own matrix
    public static ViewMatrix ReadMatrix(Swed swed, IntPtr matrixAddr)
    {
        var viewMatrix = new ViewMatrix();
        var matrix = swed.ReadMatrix(matrixAddr);

        // converting into our matrix
        // there is probably a smarter way to do this but who doesn't love hardcodíng <<

        // first row
        viewMatrix.m11 = matrix[0];
        viewMatrix.m12 = matrix[1];
        viewMatrix.m13 = matrix[2];
        viewMatrix.m14 = matrix[3];

        // second row
        viewMatrix.m21 = matrix[4];
        viewMatrix.m22 = matrix[5];
        viewMatrix.m23 = matrix[6];
        viewMatrix.m24 = matrix[7];

        // third row
        viewMatrix.m31 = matrix[8];
        viewMatrix.m32 = matrix[9];
        viewMatrix.m33 = matrix[10];
        viewMatrix.m34 = matrix[11];

        // fourth row
        viewMatrix.m41 = matrix[12];
        viewMatrix.m42 = matrix[13];
        viewMatrix.m43 = matrix[14];
        viewMatrix.m44 = matrix[15];

        return viewMatrix;
    }
}
