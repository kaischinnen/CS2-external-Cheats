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
