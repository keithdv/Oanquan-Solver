using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BoardgamSolver
{
    public class CaptureScreen
    {
        private static object lockArray = new object();

        public static async Task<byte[]> CaptureNextNumbers(RECT window)
        {
            List<Task> tasks = new List<Task>();

            byte[] results = new byte[3];

            do
            {

                Parallel.For(0, 3, x =>
                 {
                     tasks.Add(FindNumber(window, x, (x * 112) + 330, 400, results, "Top"));
                 });

                await Task.WhenAll(tasks);

            } while (results.Distinct().Count() == 1 && results.First() == 0);

            return results;
        }

        public static async Task Capture(RECT window, byte[] current, List<(int X, int Y)> spots)
        {

            List<Task> tasks = new List<Task>();

            while (spots.Count > 0)
            {
                var curSpots = spots.Take(10).ToList();
                Parallel.ForEach(curSpots, s =>
                {
                    tasks.Add(FindNumber(window, s.X + (s.Y * 6), (s.X * 146) + 69, (s.Y * 146) + 577, current));
                });
                await Task.Delay(500);
                curSpots.ForEach(s => spots.Remove(s));

            }

            await Task.WhenAll(tasks);
            tasks.Clear();
        }

        public static async Task<byte[]> Capture(RECT window)
        {

            List<Task> tasks = new List<Task>();

            byte[] results = new byte[36];

            for (int y = 0; y < 6; y += 1)
            {
                Parallel.For(0, 6, x =>
                {
                    tasks.Add(FindNumber(window, x + (y * 6), (x * 146) + 69, (y * 146) + 577, results));
                });
                await Task.Delay(1000);
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            return results;

        }



        public static int GetHash(Bitmap bmpSource)
        {

            var result = 0;

            for (int j = 0; j < bmpSource.Height; j += 1)
            {
                for (int i = 0; i < bmpSource.Width; i += 1)
                {
                    result += bmpSource.GetPixel(i, j).GetBrightness() > 0.8f ? 0 : 1;
                }
            }

            return result;

        }

        public struct FoundNumber
        {
            public int Hash { get; set; }
            public byte Number { get; set; }
        }

        public static async Task FindNumber(RECT window, int index, int X, int Y, byte[] results, string filename = "Result")
        {

            using (var screenStream = new MemoryStream())
            {
                using (var screenBmp = new Bitmap(50, 50, PixelFormat.Format32bppArgb))
                {
                    using (var bmpGraphics = Graphics.FromImage(screenBmp))
                    {
                        bmpGraphics.CopyFromScreen(window.Left + X, window.Top + Y, 0, 0, new System.Drawing.Size(45, 50));
                    }

                    screenBmp.Save(screenStream, ImageFormat.Png);

                    var hash = GetHash(screenBmp);


                    if (hash == 250)
                    {
                        // Empty
                        results[index] = 0;
                        //screenBmp.Save(@$".\{filename}_{index}_{hash}_E.png", ImageFormat.Png);

                        return;
                    }


                    screenStream.Seek(0, SeekOrigin.Begin);
                    var value = await RecognizeText.MakeOCRRequest(screenStream);


                    if ((value?.RecognitionResult.Lines.Length ?? 0) == 1)
                    {
                        if (byte.TryParse(value.RecognitionResult.Lines[0].Text, out var number))
                        {
                            lock (lockArray)
                            {
                                results[index] = number;
                            }

                            //screenBmp.Save(@$".\{filename}_{index}_{hash}_{number}.png", ImageFormat.Png);

                        }
                        else
                        {
                            lock (lockArray)
                            {
                                results[index] = 10;
                            }
                            //  screenBmp.Save(@$".\{filename}_{index}_{hash}_o.png", ImageFormat.Png);


                        }
                    }
                    else if ((hash > 450 && hash < 550) || (hash > 340 && hash < 376))
                    {
                        lock (lockArray)
                        {
                            results[index] = 1;
                        }
                        screenBmp.Save(@$".\{filename}_{index}_{hash}_1.png", ImageFormat.Png);

                    }
                    else
                    {
                        MessageBox.Show($"Miss {index} {hash}");
                        screenBmp.Save(@$".\{filename}_{index}_{hash}_Miss.png", ImageFormat.Png);

                    }
                }
            }
        }

    }
}
