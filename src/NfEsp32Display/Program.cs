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
            display.FillScreen(Color.White);
            display.Update();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            display.FillScreen(Color.Black);
            display.Update();
            //display.EraseDisplay();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
