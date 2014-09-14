using System;

namespace KinectPong
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GamePong game = new GamePong())
            {
                game.Run();
            }
        }
    }
#endif
}

