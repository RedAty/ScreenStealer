using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Timers;

namespace ScreenStealer
{
    class Program
    {
        private static string[] config = new string[] { "image", "out.bmp", "" };

        /**
         * ScreenStealer Input
         * 
         * ScreenStealer.exe type target options
         * 
         * type: screen and window (default: screen)
         * target: "console" or filepath
         * options: 
         *      if target file is HTML then we can choose "append"
         *      if target is a folder, options will be the "second" counter (around 20FPS)
         * 
         */
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            int index = 0;
            foreach( string arg in args)
            {
                config[index] = arg;
                index += 1;
            }
            Capture capture = new Capture();
            ImageFormat format = ImageFormat.Jpeg;
            string extension = ".jpeg";
            string imageType = "data:image/jpeg;base64, ";

            /*
             * Determine the input format if we can
             */
            if(config[1] != "" && config[1] != "console")
            {
                string pathExtension = Path.GetExtension(config[1]);
                if(pathExtension != "")
                {
                    imageType = "data:image/" + pathExtension.Substring(1) + ";base64, ";
                    extension = pathExtension;
                    switch (pathExtension)
                    {
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".png":
                            format = ImageFormat.Png;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            format = ImageFormat.Gif;
                            break;
                        case ".tiff":
                            format = ImageFormat.Tiff;
                            break;
                        default:
                            format = ImageFormat.Jpeg;
                            break;
                    }
                }
            }

            
            if (config[1] == "console" || config[0] == "console")
                /**
                * Base64 output to stdout
                * 
                * Example for 
                * ScreenStealer.exe console
                * ScreenStealer.exe window console
                * ScreenStealer.exe screen console
                */
            {
                Image screen = config[0] == "window" ? capture.CaptureActiveWindow() :capture.CaptureScreen();
                Console.WriteLine(imageType + ConvertImageToBase64String(screen, format));

            } if (config[1].EndsWith(".html"))
                /**
                *  HTML Output to <img></img>
                * 
                * Example for 
                * ScreenStealer.exe window filename.html append
                * ScreenStealer.exe window filename.html
                * ScreenStealer.exe screen filename.html append
                * ScreenStealer.exe screen filename.html
                */
            {
                Image screen = config[0] == "window" ? capture.CaptureActiveWindow() : capture.CaptureScreen();
                string image = imageType + ConvertImageToBase64String(screen, format);
                string startImage = "<img src=\"";
                string endImage = "\" alt = \"Image\" /> ";

                if(config[2] == "append")
                {
                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(@config[1], true))
                    {
                        file.WriteLine(startImage + image + endImage);
                    }
                } else
                {
                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(@config[1]))
                    {
                        file.WriteLine(startImage + image + endImage);
                    }
                }
                

            } else if (config[1] != "")
                /**
                *  Image file output
                *  
                *  If you provide path without extension and an integer after that,
                *  the application will create a series of images for that time period what you defined
                *  
                * 
                * Example for 
                * ScreenStealer.exe window folder/ 10   //10 seconds images
                * ScreenStealer.exe screen folder/ 10
                * ScreenStealer.exe screen imagefile.png
                * ScreenStealer.exe window imagefile.gif
                * ScreenStealer.exe window imagefile.bmp
                */
            {
                if (config[2] != "")
                {
                    int seconds = Int32.Parse(config[2]);
                    DateTime startTime = DateTime.UtcNow;
                    int i = 0;
                    if(config[0] == "window")
                    {
                        while (DateTime.UtcNow - startTime < TimeSpan.FromMinutes(seconds))
                        {
                            capture.CaptureActiveWindowToFile(config[1] + i + extension, format);
                            i += 1;
                        }
                    } 
                    else
                    {
                        Console.WriteLine("Start Async Capturing");
                        Thread thread = new Thread(capture.StartScreenSequenceCapturingLive);
                        thread.Start();
                        //capture.StartScreenSequenceCapturingLive();

                        Console.WriteLine("Start " + seconds + " seconds");
                        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(seconds))
                        {
                            // Waiting
                        }
                        Console.WriteLine("Stop Capturing");
                        //Not working properly
                        await capture.StopScreenSequenceCapturingLive();
                        Console.WriteLine("Get Captured Images");

                        Image[] sequence = capture.GetCapturedSequence();
                        foreach(Image image in sequence)
                        {
                            Console.WriteLine(config[1] + i + extension);
                            image.Save(config[1] + i + extension, format);
                            i += 1;
                        }

                        /*
                        Image[] sequence = capture.GetScreenSequenceArray(200);
                        for (int j = 0; j < sequence.Length; j++)
                        {
                            sequence[j].Save(config[1] + j + extension, format);
                        }*/
                        /*while (timer != true)
                        {

                            capture.CaptureScreenToFile(config[1] + i + extension, format);
                            i += 1;
                        }*/
                    }
                    
                } else
                {
                    if(config[0] == "window")
                    {
                        capture.CaptureActiveWindowToFile(config[1], format);
                    } else
                    {
                        capture.CaptureScreenToFile(config[1], format);

                    }
                }
                

            }
        }
       
        public static string ConvertImageToBase64String(Image image, ImageFormat format)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return Convert.ToBase64String(ms.ToArray());
            }
        }



    }
}
