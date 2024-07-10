using System;

namespace OpenGL.App
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                //game.VSync = OpenTK.Windowing.Common.VSyncMode.Adaptive;
                game.Run();
            }
        }
    }
}
