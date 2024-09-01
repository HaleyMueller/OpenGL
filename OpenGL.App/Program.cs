using OpenCvSharp;
using System;
using static OpenGL.App.Game;
using System.Drawing;
using System.Collections.Concurrent;
using static OpenGL.App.FFMPEG;

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
