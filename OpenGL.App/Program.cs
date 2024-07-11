using OpenCvSharp;
using System;
using static OpenGL.App.Game;
using System.Drawing;
using System.Collections.Concurrent;

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

        public static float ScaleFactor;
        public static Video ExtractFrames(string videoPath, float scaleFactor)
        {
            ScaleFactor = scaleFactor;
            var video = new Video();
            using (var capture = new VideoCapture(videoPath))
            {
                if (!capture.IsOpened())
                {
                    Console.WriteLine("Failed to open video file.");
                    return null;
                }

                video.FPS = capture.Fps;
                video.Frames = new Frame[capture.FrameCount];

                int frameNumber = 0;
                Mat frame = new Mat();

                while (true)
                {
                    capture.Read(frame);
                    if (frame.Empty())
                        break;

                    using (Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame))
                    {
                        // Calculate new dimensions
                        int newWidth = (int)(bitmap.Width * scaleFactor);
                        int newHeight = (int)(bitmap.Height * scaleFactor);

                        video.Width = newWidth;
                        video.Height = newHeight;

                        if (scaleFactor == 1)
                        {
                            ConvertFrameToBoolArray(bitmap, frameNumber, ref video);
                        }
                        else
                        {

                            // Create a new bitmap with the new dimensions
                            using (Bitmap resizedBitmap = new Bitmap(newWidth, newHeight))
                            {
                                // Draw the original bitmap onto the new bitmap
                                using (Graphics graphics = Graphics.FromImage(resizedBitmap))
                                {
                                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    graphics.DrawImage(bitmap, 0, 0, newWidth, newHeight);
                                }
                                ConvertFrameToBoolArray(resizedBitmap, frameNumber, ref video);
                            }
                        }
                        //SaveFrameAsBitmap(bitmap, frameNumber);
                    }

                    Console.WriteLine($"{frameNumber} / {capture.FrameCount} {((float)frameNumber / (float)capture.FrameCount) * 100}%");

                    frameNumber++;
                }
            }

            return video;
        }

        static bool[,] ConvertFrameToBoolArray(Bitmap frame, int frameNumber, ref Video video)
        {
            int width = frame.Width;
            int height = frame.Height;
            bool[,] boolArray = new bool[width, height];

            // Iterate over each pixel to create the boolean array
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = frame.GetPixel(x, y);
                    boolArray[x, y] = pixelColor.R == 255;
                }
            }

            video.Frames[frameNumber] = new Frame();
            //video.Frames[frameNumber].Pixels = new bool[width,height];
            video.Frames[frameNumber].Pixels = boolArray;

            return boolArray;
        }
    }
}
