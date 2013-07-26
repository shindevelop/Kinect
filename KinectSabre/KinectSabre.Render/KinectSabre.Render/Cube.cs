using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectSabre.Render
{
    public class Cube
    {
        BasicEffect basicEffect;
        GraphicsDevice device;

        public Cube(GraphicsDevice device)
        {
            this.device = device;
            World = Matrix.Identity;
            basicEffect = new BasicEffect(device);

            CreateBuffers();
        }

        private static readonly VertexPositionColor[] _vertices = new[]
        {
            //right
            new VertexPositionColor(new Vector3( 1,  1,  1), Color.White.ToVector4()),
            new VertexPositionColor(new Vector3( 1,  1, -1), Color.White.ToVector4()),
            new VertexPositionColor(new Vector3( 1, 0,  1), Color.White.ToVector4()),
            new VertexPositionColor(new Vector3( 1, 0, -1), Color.White.ToVector4()),

            //left
            new VertexPositionColor(new Vector3(-1,  1, -1), Color.White.ToVector4()),
            new VertexPositionColor(new Vector3(-1,  1,  1), Color.White.ToVector4()),
            new VertexPositionColor(new Vector3(-1, 0, -1), Color.White.ToVector4()),
            new VertexPositionColor(new Vector3(-1, 0,  1), Color.White.ToVector4())
        };

        static readonly int[] _indices = new[]
        {
            0,1,2,
            2,1,3,

            4,5,6,
            6,5,7,

            5,1,0,
            5,4,1,

            7,3,2,
            7,6,3,

            4,3,1,
            4,6,3,

            5,0,2,
            5,7,2
        };

        private VertexBuffer _vb;
        private IndexBuffer _ib;

        public void CreateBuffers()
        {
            _vb = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, _vertices.Length, BufferUsage.None);
            _vb.SetData(_vertices);
            _ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, _indices.Length, BufferUsage.None);
            _ib.SetData(_indices);
        }

        public Matrix World { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public void Draw()
        {
            basicEffect.World = World;
            basicEffect.View = View;
            basicEffect.Projection = Projection;
            basicEffect.VertexColorEnabled = true;

            device.SetVertexBuffer(_vb);
            device.Indices = _ib;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vb.VertexCount, 0, _indices.Length / 3);
            }
        }
    }
}
