using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Neoshade;


public static class InternalSystem {
  [DllImport("user32.dll", SetLastError = true)]
   [return: MarshalAs(UnmanagedType.Bool)]
   static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
   
    
    public static unsafe void Draw(byte[] image, int width, int height, int x = 0, int y = 0, nint? window = null) {
        uint bitSize = 8;        
        int totalSize = (int)((((width * 4 * bitSize + 15) >> 4) << 1) * height);
        if(totalSize > image.Length) {
            throw new IndexOutOfRangeException();
        }
        // according to documentation User32 GetDC with parameter 0 returns current screen
        HDC screenHandler = PInvoke.GetDC(window == null ? PInvoke.FindWindow(null, "Roblox") : new HWND(window.GetValueOrDefault()));


        IntPtr imageBuffer = Marshal.AllocHGlobal(totalSize);
        Marshal.Copy(image, 0, imageBuffer, totalSize);
        HBITMAP bitmap = PInvoke.CreateBitmap(width, height, 4, bitSize, imageBuffer.ToPointer());
        HDC bitmapDisplayContext = PInvoke.CreateCompatibleDC(screenHandler);
        // PInvoke.DwmRegisterThumbnail()
        PInvoke.SelectObject(bitmapDisplayContext, bitmap);
        
        PInvoke.BitBlt(screenHandler, x, y, width, height, bitmapDisplayContext, 0, 0, ROP_CODE.SRCCOPY | ROP_CODE.CAPTUREBLT);
        PInvoke.DeleteDC(bitmapDisplayContext);
        PInvoke.DeleteObject(bitmap);
        PInvoke.DeleteDC(screenHandler);
        Marshal.FreeHGlobal(imageBuffer);
    }

    public static Bitmap GetFramebuffer(int width, int height, int x = 0, int y = 0) {
        int length = width * height * 4;
        byte[] data = new byte[length];

        HDC screenHandler = PInvoke.GetDC(PInvoke.FindWindow(null, "Roblox"));
        
        HBITMAP bitmap = PInvoke.CreateCompatibleBitmap(screenHandler, width, height);
        HDC bitmapDisplayContext = PInvoke.CreateCompatibleDC(screenHandler);
        PInvoke.SelectObject(bitmapDisplayContext, bitmap);        
        // PInvoke.BitBlt(bitmapDisplayContext, x, y, width, height, screenHandler, 0, 0, ROP_CODE.SRCCOPY);
        PrintWindow(PInvoke.FindWindow(null, "Roblox"), bitmapDisplayContext, 3);

        Bitmap bitmap1 = Bitmap.FromHbitmap(bitmap);
        PInvoke.DeleteDC(bitmapDisplayContext);
        PInvoke.DeleteObject(bitmap);        
        PInvoke.DeleteDC(screenHandler);

        // BitmapData bitmapData = bitmap1.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        // Marshal.Copy(bitmapData.Scan0, data, 0, length);
        return bitmap1;
    }

    public static byte[] BitmapToByte(Bitmap bitmap) {
        int width = bitmap.Width;
        int height = bitmap.Height;
        int length = width * height * 4;

        byte[] data = new byte[length];
        
        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        Marshal.Copy(bitmapData.Scan0, data, 0, length);        

        bitmap.UnlockBits(bitmapData);
        return data;
    }
    
}