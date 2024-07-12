using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OpenGL.App.FFMPEG;

namespace OpenGL.App
{
    public class FFMPEG
    {
        public async Task<Video> MakeVideoIntoBitmaps(string videoPath, string tempPath)
        {
            var vid = await _MakeVideoIntoBitmaps(videoPath, tempPath);
            return vid;
        }

        private async Task<Video> _MakeVideoIntoBitmaps(string videoPath, string tempPath)
        {
            var ret = new Video();
            // Create a new process
            Process process = new Process();
            process.StartInfo.FileName = GetFFmpegPath();
            process.StartInfo.Arguments = $"-i {videoPath} {tempPath}/$filename%03d.bmp";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            List<string> recheckFiles = new List<string>();

            TimeSpan duration = TimeSpan.MinValue;

            // Capture standard output and error
            process.OutputDataReceived += (sender, e) =>
            {
                Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += async (sender, e) =>
            {
                Console.WriteLine(e.Data);
                if (e.Data != null)
                {
                    if (e.Data.Contains("fps"))
                    {
                        if (ret.FPS == 0)
                        {
                            var match = Regex.Match(e.Data, @"(\d+)\s*fps");
                            if (match.Success)
                            {
                                ret.FPS = int.Parse(match.Groups[1].Value);
                            }
                        }

                        

                        if (ret.Height == 0)
                        {
                            var match = Regex.Match(e.Data, @"\b(\d{2,4}x\d{2,4})\b");
                            if (match.Success)
                            {
                                var w = match.Groups[1].Value.Split('x')[0];
                                var h = match.Groups[1].Value.Split('x')[1];
                                ret.Width = int.Parse(w);
                                ret.Height = int.Parse(h);
                            }
                        }
                    }

                    if (e.Data.Contains("Duration"))
                    {
                        if (ParseDuration(e.Data) != TimeSpan.MinValue)
                        {
                            duration = ParseDuration(e.Data);
                        }
                    }
                }
            };

            try
            {
                // Start the process
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the process to exit
                process.WaitForExit();

                int bmps = 0;

                Console.WriteLine("Starting to load bmps...");

                List<string> imagePaths = Directory.GetFiles("tmp", "*.bmp").ToList();

                // List to store loaded bitmaps
                ConcurrentBag<Test> bitmaps = new ConcurrentBag<Test>();

                // Define parallel options with a limited degree of parallelism
                ParallelOptions parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount // Limit the number of concurrent tasks
                };

                Console.WriteLine($"Started at {DateTime.Now.ToShortTimeString()}");

                // Use Parallel.ForEach to load bitmaps in parallel
                Parallel.ForEach(imagePaths, parallelOptions, imagePath =>
                {
                    try
                    {
                        Bitmap bitmap = new Bitmap(imagePath);
                        var frameNumber = 0;
                        var match = Regex.Match(imagePath, "-?\\d+\\.?\\d*");
                        if (match.Success)
                        {
                            frameNumber = int.Parse(match.Groups[0].Value.Replace(".", ""))-1;
                        }
                        else
                        {
                            Console.WriteLine("Couldn't find frame number for: " + imagePaths);
                        }

                        bitmaps.Add(new Test() { Bools = ConvertFrameToBoolArray(bitmap, frameNumber), index = frameNumber });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load image {imagePath}: {ex.Message}");
                    }
                });

                //ret.Frames = new Frame[bitmaps.Count];

                var frames = (int)Math.Round((duration.TotalSeconds * ret.FPS), MidpointRounding.AwayFromZero);

                ret.Frames = new Frame[frames];

                foreach (var test in bitmaps)
                {
                    ret.Frames[test.index] = new Frame();
                    ret.Frames[test.index].Pixels = test.Bools;
                }

                Console.WriteLine($"Ended at {DateTime.Now.ToShortTimeString()}");

                Console.WriteLine($"Finished loading {bmps} bmps");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return ret;
        }

        public class Test
        {
            public bool[,] Bools { get; set; }
            public int index { get; set; }
        }

        static TimeSpan ParseDuration(string ffmpegOutput)
        {
            // Regular expression to match the duration information
            Regex durationRegex = new Regex(@"Duration: (\d{2}):(\d{2}):(\d{2})\.(\d{2})", RegexOptions.Compiled);
            Match match = durationRegex.Match(ffmpegOutput);

            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                int seconds = int.Parse(match.Groups[3].Value);
                int milliseconds = int.Parse(match.Groups[4].Value);

                return new TimeSpan(0, hours, minutes, seconds, milliseconds);

                double totalSeconds = hours * 3600 + minutes * 60 + seconds + milliseconds / 100.0;
                //return totalSeconds.ToString();
            }
            else
            {
                throw new FormatException("Duration information not found in FFmpeg output.");
            }
        }

        static bool[,] ConvertFrameToBoolArray(Bitmap frame, int frameNumber)
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

            return boolArray;
        }

        static int ExtractValue(string output, string key)
        {
            var match = Regex.Match(output, $@"{key}=\s*(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : -1;
        }

        public async Task<bool[,]> ReadBitmapFromOutput(string output, int width, int height)
        {
            try
            {
                bool[,] boolArray = new bool[width, height];

                var bmp = new Bitmap(output);

                // Iterate over each pixel to create the boolean array
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixelColor = bmp.GetPixel(x, y);
                        boolArray[x, y] = pixelColor.R == 255;
                    }
                }

                Console.WriteLine("Got bmp");

                return boolArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        static string GetFFmpegPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return @"lib/ffmpeg/windows/\ffmpeg.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.Arm64:
                        return "lib/ffmpeg/linux/arm64/ffmpeg";
                    case Architecture.X64:
                        return "lib/ffmpeg/linux/amd64/ffmpeg";
                    default:
                        throw new PlatformNotSupportedException("Unsupported OS platform for ffmpeg");
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "lib/ffmpeg/apple/ffmpeg";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform for ffmpeg");
            }
        }

        public class Frame
        {
            public bool[,] Pixels { get; set; }
        }

        public class Video
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public double FPS { get; set; }
            public Frame[] Frames { get; set; }
        }
    }
}
