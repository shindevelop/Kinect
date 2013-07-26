using System;

namespace KinectSabre.Render
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (RenderGame game = new RenderGame())
            {
                game.Run();
            }
        }
    }
#endif
}

