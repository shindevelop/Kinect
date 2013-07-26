using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectSabre.Render
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;

        public static readonly VertexDeclaration VertexDeclaration;


        public VertexPositionColor(Vector3 pos, Vector4 color)
        {
            Position = pos;
            Color = color;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPositionColor()
        {
            VertexElement[] elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position,0),
                new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Color,0)
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
