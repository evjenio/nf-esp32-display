using System;
using System.Threading;
using NfEsp32Display.Epaper;

#nullable enable

namespace NfEsp32Display
{
    public class Program
    {
        public static void Main()
        {
            var display = new Display();
            display.Init();
            display.SetRotation(0);

            display.FillScreen(Color.White);
            for (int x = 0; x < display.Width; x++)
            {
                display.DrawPixel(x, 249, Color.Black);
                display.DrawPixel(x, 0, Color.Black);
            }

            display.UpdateWindow(0, 0, display.Width, display.Height);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            display.FastClear();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
