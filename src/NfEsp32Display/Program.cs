using System.Threading;
using NfEsp32Display.Epaper;
using NfEsp32Display.Resources;

#nullable enable

namespace NfEsp32Display
{
    using Img = Bitmaps.Num1;

    public class Program
    {
        public static void Main()
        {
            using var display = new Display();
            display.Init();
            display.EraseDisplayFast();

            display.SetRotation(1);

            display.SetFontSize(3);
            display.SetCursor(0, 0);

            display.FillScreen(Color.White);
            display.DrawBitmap(Img.Bitmap, 0, 28, Img.Width, Img.Heigth, Color.White);
            display.DrawBitmap(Img.Bitmap, Img.Width + 0, 28, Img.Width, Img.Heigth, Color.White);
            display.DrawBitmap(Img.Bitmap, 2 * (Img.Width + 0), 28, Img.Width, Img.Heigth, Color.White);
            display.DrawBitmap(Img.Bitmap, 3 * (Img.Width + 0), 28, Img.Width, Img.Heigth, Color.White);

            display.Write("Hello world!");
            display.UpdateWindow(0, 0, display.Width, display.Height);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
