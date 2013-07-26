using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace Kinect.Toolkit
{
    public static class Tools
    {
        public static Vector3 ToVector3(this Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        public static Vector2 ToVector2(this Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
