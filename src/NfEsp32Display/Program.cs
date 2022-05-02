using System.Threading;
using NfEsp32Display.Epaper;
using NfEsp32Display.Resources;

#nullable enable

namespace NfEsp32Display
{
    using Dino = Bitmaps.Dino;

    public class Program
    {
        public static void Main()
        {
            using var display = new Display();
            display.Init();
            display.SetRotation(1);

            display.SetFontSize(2);
            display.SetCursor(110, 35);

            display.FillScreen(Color.White);
            display.DrawBitmap(Dino.Bitmap, 0, 0, Dino.Width, Dino.Heigth, Color.White);
            display.Write("Hello world!");
            display.UpdateWindow(0, 0, display.Width, display.Height);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
