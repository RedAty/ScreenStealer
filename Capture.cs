using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ScreenStealer
{
    class Capture
    {
        private IntPtr[] sequence;
        private bool started = false;
        private List<IntPtr> capturedData;

        private IntPtr hdcSrc;
        //private IntPtr windowRect;
        private int width;
        private int height;

        /**
         * I created Async capturing with come cache to improve FPS numbers a little bit
         * 
        * References:
        * https://gallery.technet.microsoft.com/scriptcenter/eeff544a-f690-4f6b-a586-11eea6fc5eb8
        * https://www.hanselman.com/blog/HowDoYouUseSystemDrawingInNETCore.aspx
        */
        /// <summary> 
        /// Creates an Image object containing a screen shot the active window 
        /// </summary> 
        /// <returns></returns> 
        public Image CaptureActiveWindow()
        {
            return CaptureWindow(User32.GetForegroundWindow());
        }
        /// <summary> 
        /// Creates an Image object containing a screen shot of the entire desktop 
        /// </summary> 
        /// <returns></returns> 
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

         public async void StartScreenSequenceCapturingLive()
        {
            capturedData = new List<IntPtr>();
            IntPtr window = User32.GetDesktopWindow();
            CacheWindowData(window);
            started = true;
            while (started == true)
            {
                capturedData.Add(CaptureCachedPointer(window));
            }

        }

        public async Task StopScreenSequenceCapturingLive()
        {
            started = false;
        }

        public Image[] GetCapturedSequence()
        {
            int count = capturedData.Count;
            Image[] images = new Image[count];
            Console.WriteLine(count);
            for (int runs = 0; runs < count; runs++)
            {
                images[runs] = Image.FromHbitmap(capturedData[runs]);
            }
            return images;
        }

        public Image[] GetScreenSequenceArray(int size)
        {
            return PointerArrayToBitmap(StartScreenSequenceCapturingToArray(size));
        }

        public IntPtr[] StartScreenSequenceCapturingToArray(int size)
        {
            sequence = new IntPtr[size];
            IntPtr window = User32.GetDesktopWindow();
            CacheWindowData(window);
            for (int runs = 0; runs < size; runs++)
            {
                sequence[runs] = CaptureCachedPointer(window);
            }
            return sequence;
        }

        public Image[] PointerArrayToBitmap(IntPtr[] sequence)
        {
            Console.WriteLine(sequence.Length + " IntPtr");
            Image[] images = new Image[sequence.Length];
            for (int runs = 0; runs < sequence.Length; runs++)
            {
                images[runs] = Image.FromHbitmap(sequence[runs]);
            }
            return images;
        }
        private void CacheWindowData(IntPtr handle)
        {
            // get te hDC of the target window 
            hdcSrc = User32.GetWindowDC(handle);
            // get the size 
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            width = windowRect.right - windowRect.left;
            height = windowRect.bottom - windowRect.top;
        }

        private IntPtr CaptureCachedPointer(IntPtr handle)
        {
            // get te hDC of the target window 
            hdcSrc = User32.GetWindowDC(handle);
            // create a device context we can copy to 
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to, 
            // using GetDeviceCaps to get the width/height 
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object 
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over 
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection 
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            return hBitmap;
        }
        /// <summary> 
        /// Creates an Image object containing a screen shot of a specific window 
        /// </summary> 
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param> 
        /// <returns></returns> 
        private Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window 
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size 
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to 
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to, 
            // using GetDeviceCaps to get the width/height 
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object 
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over 
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection 
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it 
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object 
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary> 
        /// Captures a screen shot of the active window, and saves it to a file 
        /// </summary> 
        /// <param name="filename"></param> 
        /// <param name="format"></param> 
        public void CaptureActiveWindowToFile(string filename, ImageFormat format)
        {
            Image img = CaptureActiveWindow();
            img.Save(filename, format);
        }
        /// <summary> 
        /// Captures a screen shot of the entire desktop, and saves it to a file 
        /// </summary> 
        /// <param name="filename"></param> 
        /// <param name="format"></param> 
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }

        /// <summary> 
        /// Helper class containing Gdi32 API functions 
        /// </summary> 
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter 
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
              int nWidth, int nHeight, IntPtr hObjectSource,
              int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
              int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary> 
        /// Helper class containing User32 API functions 
        /// </summary> 
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
        }


    }
}
