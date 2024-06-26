﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;

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
