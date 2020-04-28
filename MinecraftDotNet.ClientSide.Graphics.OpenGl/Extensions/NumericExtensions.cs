using System.Drawing;
using System.Numerics;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl.Extensions
{
    using OglSize = OpenTK.Size;
    using OglVector3 = OpenTK.Vector3;
    
    public static class NumericExtensions
    {
        public static Size ToSys(this OglSize oglSize)
        {
            return new Size(oglSize.Width, oglSize.Height);
        }

        public static Vector3 ToSys(this OglVector3 oglVec)
        {
            return new Vector3(oglVec.X, oglVec.Y, oglVec.Z);
        }
    }
}