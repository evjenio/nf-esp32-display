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

            while (true)
            {
                display.FillScreen(Color.White);
                display.UpdateWindow(0, 0, display.Width, display.Height);

                Thread.Sleep(TimeSpan.FromSeconds(5));

                display.FillScreen(Color.Black);
                display.UpdateWindow(0, 0, display.Width, display.Height);

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
