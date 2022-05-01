using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;

namespace NfEsp32Display.Epaper
{
    public class Display : IDisposable
    {
        private readonly SpiDevice spiDevice;
        private readonly GpioController gpioController;
        private readonly GpioPin busyPin;
        private readonly GpioPin resetPin;
        private readonly GpioPin dcPin;

        // the physical number of pixels (for controller parameter)
        const int GDEH0213B73_X_PIXELS = 128;
        const int GDEH0213B73_Y_PIXELS = 250;

        // the logical width and height of the display
        const int GDEH0213B73_WIDTH = GDEH0213B73_X_PIXELS;
        const int GDEH0213B73_HEIGHT = GDEH0213B73_Y_PIXELS;

        // note: the visible number of display pixels is 122*250, see GDEH0213B72 V1.1 Specification.pdf
        const int GDEH0213B73_VISIBLE_WIDTH = 122;

        const int GDEH0213B73_BUFFER_SIZE = (GDEH0213B73_WIDTH) * (GDEH0213B73_HEIGHT) / 8;

        const int SPI_MOSI = 23;
        // const int SPI_MISO = -1;
        const int SPI_CLK = 18;

        const int ELINK_CS = 5;
        const int ELINK_BUSY = 4;
        const int ELINK_RESET = 16;
        const int ELINK_DC = 17;

        byte[] _buffer = new byte[GDEH0213B73_BUFFER_SIZE];

        private readonly byte[] LUT_DATA_full =
        {
          0xA0,  0x90, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x50, 0x90, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0xA0, 0x90, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x50, 0x90, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

          0x0F, 0x0F, 0x00, 0x00, 0x00,
          0x0F, 0x0F, 0x00, 0x00, 0x03,
          0x0F, 0x0F, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,

          //0x15, 0x41, 0xA8, 0x32, 0x50, 0x2C, 0x0B,
        };

        private readonly byte[] LUT_DATA_part =
        {
          0x40,  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

          0x0A, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00,

          //0x15, 0x41, 0xA8, 0x32, 0x50, 0x2C, 0x0B,
        };

        public Display()
        {
            gpioController = new GpioController();
            busyPin = gpioController.OpenPin(ELINK_BUSY, PinMode.Input);
            resetPin = gpioController.OpenPin(ELINK_RESET, PinMode.Output);
            dcPin = gpioController.OpenPin(ELINK_DC, PinMode.Output);

            Configuration.SetPinFunction(SPI_MOSI, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(SPI_CLK, DeviceFunction.SPI1_CLOCK);

            var connectionSettings = new SpiConnectionSettings(1, ELINK_CS)
            {
                ClockFrequency = 4_000_000, // 4MHz
                Mode = SpiMode.Mode0,
                DataFlow = DataFlow.MsbFirst,
            };

            spiDevice = new SpiDevice(connectionSettings);
        }

        public int Width => GDEH0213B73_WIDTH;
        public int Height => GDEH0213B73_HEIGHT;

        public void Dispose()
        {
            spiDevice.Dispose();
            gpioController.Dispose();
            busyPin.Dispose();
            resetPin.Dispose();
            dcPin.Dispose();
        }

        public void Init()
        {
            HwReset();

            WaitWhileBusy("HW reset");
            WriteCommand(0x12); // sw reset
            WaitWhileBusy("SW reset");
        }

        public void FillScreen(Color color)
        {
            for (var x = 0; x < _buffer.Length; x++)
            {
                _buffer[x] = (byte)color;
            }
        }

        public void Update()
        {
            InitFull(0x03);

            WriteCommand(0x24);
            WriteBuffer();

            WriteCommand(0x26);
            WriteBuffer();

            UpdateFull();
            PowerOff();

            void WriteBuffer()
            {
                for (ushort y = 0; y < GDEH0213B73_HEIGHT; y++)
                {
                    for (ushort x = 0; x < GDEH0213B73_WIDTH / 8; x++)
                    {
                        ushort idx = (ushort)(y * (GDEH0213B73_WIDTH / 8) + x);
                        byte data = (idx < _buffer.Length) ? _buffer[idx] : (byte)0x00;
                        WriteData(data);
                    }
                }
            }
        }

        public void UpdateWindow(int x, int y, int w, int h)
        {
            if (x >= GDEH0213B73_WIDTH) return;
            if (y >= GDEH0213B73_HEIGHT) return;
            var xe = Math.Min(GDEH0213B73_WIDTH, x + w) - 1;
            var ye = Math.Min(GDEH0213B73_HEIGHT, y + h) - 1;
            var xs_d8 = x / 8;
            var xe_d8 = xe / 8;
            
            InitPart(0x03);
            WriteBufferWithCommand(0x24);

            UpdatePart();
            Thread.Sleep(300);
            // update erase buffer
            WriteBufferWithCommand(0x26);
            Thread.Sleep(300);

            void WriteBufferWithCommand(byte command)
            {
                SetRamArea((byte)xs_d8, (byte)xe_d8, (byte)(y % 256), (byte)(y / 256), (byte)(ye % 256), (byte)(ye / 256)); // X-source area,Y-gate area
                SetRamPointer((byte)xs_d8, (byte)(y % 256), (byte)(y / 256)); // set ram
                WaitWhileBusy();
                WriteCommand(command);
                for (var y1 = y; y1 <= ye; y1++)
                {
                    for (var x1 = xs_d8; x1 <= xe_d8; x1++)
                    {
                        var idx = y1 * (GDEH0213B73_WIDTH / 8) + x1;
                        byte data = (idx < _buffer.Length) ? _buffer[idx] : (byte)0x00;
                        WriteData(data);
                    }
                }
            }
        }

        public void EraseDisplay()
        {
            InitFull(0x01);
            WriteCommand(0x24);
            for (int i = 0; i < GDEH0213B73_BUFFER_SIZE; i++)
            {
                WriteData(0xFF);
            }
            // update erase buffer
            WriteCommand(0x26);
            for (int i = 0; i < GDEH0213B73_BUFFER_SIZE; i++)
            {
                WriteData(0xFF);
            }
            UpdateFull();
            PowerOff();
        }


        private void HwReset()
        {
            resetPin.Write(PinValue.Low);
            Thread.Sleep(10);
            resetPin.Write(PinValue.High);
            Thread.Sleep(10);
        }

        private void WaitWhileBusy(string comment = null)
        {
            bool debug = true;
            while (IsBusy())
            {
                if (debug)
                {
                    Debug.WriteLine($"EPD: Busy: {comment} ...");
                    debug = false;
                }
            }
        }

        private bool IsBusy() => busyPin.Read() == PinValue.High;

        private void WriteCommand(byte command)
        {
            if (IsBusy())
            {
                Debug.WriteLine($"EPD: busy before command {command:X}");
                WaitWhileBusy();
            }
            dcPin.Write(PinValue.Low);
            spiDevice.WriteByte(command);
            dcPin.Write(PinValue.High);
        }

        private void WriteData(byte data)
        {
            spiDevice.WriteByte(data);
        }

        private void WriteData(byte[] data)
        {
            spiDevice.Write(data);
        }

        private void InitFull(byte em)
        {
            InitDisplay(em);
            WriteCommand(0x32);
            WriteData(LUT_DATA_full);
            PowerOn();
        }

        private void InitPart(byte em)
        {
            InitDisplay(em);
            WriteCommand(0x2C); //VCOM Voltage
            WriteData(0x26);    // NA ??
            WriteCommand(0x32);
            WriteData(LUT_DATA_part);
            PowerOn();
        }

        private void UpdateFull()
        {
            WriteCommand(0x22);
            WriteData(0xc7);
            WriteCommand(0x20);
            WaitWhileBusy("Full update");
        }

        private void UpdatePart()
        {
            WriteCommand(0x22);
            WriteData(0x04); // use Mode 1 for EPD
            WriteCommand(0x20);
            WaitWhileBusy("Part update");
        }

        private void InitDisplay(byte em)
        {
            WriteCommand(0x74); //set analog block control
            WriteData(0x54);
            WriteCommand(0x7E); //set digital block control
            WriteData(0x3B);
            WriteCommand(0x01); //Driver output control
            WriteData(0xF9);
            WriteData(0x00);
            WriteData(0x00);
            WriteCommand(0x11); //data entry mode
            WriteData(0x01);
            WriteCommand(0x44); //set Ram-X address start/end position
            WriteData(0x00);
            WriteData(0x0F);    //0x0C-->(15+1)*8=128
            WriteCommand(0x45); //set Ram-Y address start/end position
            WriteData(0xF9);   //0xF9-->(249+1)=250
            WriteData(0x00);
            WriteData(0x00);
            WriteData(0x00);
            WriteCommand(0x3C); //BorderWavefrom
            WriteData(0x03);
            WriteCommand(0x2C); //VCOM Voltage
            WriteData(0x50);    //
            WriteCommand(0x03); //Gate Driving voltage Control
            WriteData(0x15);    // 19V
            WriteCommand(0x04); //Source Driving voltage Control
            WriteData(0x41);    // VSH1 15V
            WriteData(0xA8);    // VSH2 5V
            WriteData(0x32);    // VSL -15V
            WriteCommand(0x3A); //Dummy Line
            WriteData(0x2C);
            WriteCommand(0x3B); //Gate time
            WriteData(0x0B);
            WriteCommand(0x4E); // set RAM x address count to 0;
            WriteData(0x00);
            WriteCommand(0x4F); // set RAM y address count to 0X127;
            WriteData(0xF9);
            WriteData(0x00);
            SetRamDataEntryMode(em);
        }

        private void SetRamDataEntryMode(byte em)
        {
            const ushort xPixelsPar = GDEH0213B73_X_PIXELS - 1;
            const ushort yPixelsPar = GDEH0213B73_Y_PIXELS - 1;
            em = (byte)Math.Min(em, 0x03);
            WriteCommand(0x11);
            WriteData(em);
            switch (em)
            {
                case 0x00: // x decrease, y decrease
                    SetRamArea(xPixelsPar / 8, 0x00, yPixelsPar % 256, yPixelsPar / 256, 0x00, 0x00);  // X-source area,Y-gate area
                    SetRamPointer(xPixelsPar / 8, yPixelsPar % 256, yPixelsPar / 256); // set ram
                    break;
                case 0x01: // x increase, y decrease : as in demo code
                    SetRamArea(0x00, xPixelsPar / 8, yPixelsPar % 256, yPixelsPar / 256, 0x00, 0x00);  // X-source area,Y-gate area
                    SetRamPointer(0x00, yPixelsPar % 256, yPixelsPar / 256); // set ram
                    break;
                case 0x02: // x decrease, y increase
                    SetRamArea(xPixelsPar / 8, 0x00, 0x00, 0x00, yPixelsPar % 256, yPixelsPar / 256);  // X-source area,Y-gate area
                    SetRamPointer(xPixelsPar / 8, 0x00, 0x00); // set ram
                    break;
                case 0x03: // x increase, y increase : normal mode
                    SetRamArea(0x00, xPixelsPar / 8, 0x00, 0x00, yPixelsPar % 256, yPixelsPar / 256);  // X-source area,Y-gate area
                    SetRamPointer(0x00, 0x00, 0x00); // set ram
                    break;
            }
        }

        private void SetRamArea(byte Xstart, byte Xend, byte Ystart, byte Ystart1, byte Yend, byte Yend1)
        {
            WriteCommand(0x44);
            WriteData(Xstart);
            WriteData(Xend);
            WriteCommand(0x45);
            WriteData(Ystart);
            WriteData(Ystart1);
            WriteData(Yend);
            WriteData(Yend1);
        }

        private void SetRamPointer(byte addrX, byte addrY, byte addrY1)
        {
            WriteCommand(0x4e);
            WriteData(addrX);
            WriteCommand(0x4f);
            WriteData(addrY);
            WriteData(addrY1);
        }

        private void PowerOn()
        {
            WriteCommand(0x22);
            WriteData(0xc0);
            WriteCommand(0x20);
            WaitWhileBusy();
        }

        private void PowerOff()
        {
            WriteCommand(0x22);
            WriteData(0xc3);
            WriteCommand(0x20);
            WaitWhileBusy();
        }
    }
}
