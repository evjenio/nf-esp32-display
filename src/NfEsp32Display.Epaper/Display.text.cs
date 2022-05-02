using System;

namespace NfEsp32Display.Epaper
{
    public partial class Display
    {
        private int cursorX;     ///< x location to start print()ing text
        private int cursorY;     ///< y location to start print()ing text
        private Color textColor = Color.Black;
        private Color textBackgroundColor = Color.White;
        private int textSizeX = 7;
        private int textSizeY = 7;

        public void SetCursor(int x, int y)
        {
            cursorX = x;
            cursorY = y;
        }

        public void SetFontSize(int size)
        {
            textSizeX = size > 0 ? size : 1;
            textSizeY = size > 0 ? size : 1;
        }

        public void SetTextColor(Color color)
        {
            textColor = color;
        }

        public void SetTextBackgroundColor(Color color)
        {
            textBackgroundColor = color;
        }

        public void Write(string text)
        {
            foreach (char c in text)
            {
                Write(c);
            }
        }

        public void WriteLine(string text)
        {
            Write(text);
            Write('\n');
        }

        public void Write(char c)
        {

            if (c == '\n') // Newline?
            {
                cursorX = 0;               // Reset x to zero,
                cursorY += textSizeY * 8; // advance y one line
            }
            else if (c != '\r') // Ignore carriage returns
            {

                DrawChar(cursorX, cursorY, c, textColor, textBackgroundColor, textSizeX, textSizeY);
                cursorX += textSizeX * 6; // Advance x one char
            }
        }

        private void DrawChar(int x, int y, char c, Color color, Color bg, int size_x, int size_y)
        {
            // 'Classic' built-in font

            if ((x >= Width) ||              // Clip right
                (y >= Height) ||             // Clip bottom
                ((x + 6 * size_x - 1) < 0) || // Clip left
                ((y + 8 * size_y - 1) < 0))   // Clip top
                return;


            for (var i = 0; i < 5; i++)
            { // Char bitmap = 5 columns
                var line = Fonts.Default[c * 5 + i];
                for (var j = 0; j < 8; j++, line >>= 1)
                {
                    if ((line & 1) > 0)
                    {
                        if (size_x == 1 && size_y == 1)
                            DrawPixel(x + i, y + j, color);
                        else
                            WriteFillRect(x + i * size_x, y + j * size_y, size_x, size_y, color);
                    }
                    else if (bg != color)
                    {
                        if (size_x == 1 && size_y == 1)
                            DrawPixel(x + i, y + j, bg);
                        else
                            WriteFillRect(x + i * size_x, y + j * size_y, size_x, size_y, bg);
                    }
                }
            }
            if (bg != color)
            { // If opaque, draw vertical line for last column
                if (size_x == 1 && size_y == 1)
                    WriteFastVLine(x + 5, y, 8, bg);
                else
                    WriteFillRect(x + 5 * size_x, y, size_x, 8 * size_y, bg);
            }
        }

        public void WriteFillRect(int x, int y, int w, int h, Color color)
        {
            for (var i = x; i < x + w; i++)
            {
                WriteFastVLine(i, y, h, color);
            }
        }

        public void WriteFastVLine(int x, int y, int h, Color color)
        {
            WriteLine(x, y, x, y + h - 1, color);
        }

        public void WriteFastHLine(int x, int y, int w, Color color)
        {
            WriteLine(x, y, x + w - 1, y, color);
        }

        public void DrawRect(int x, int y, int w, int h, Color color)
        {
            WriteFastHLine(x, y, w, color);
            WriteFastHLine(x, y + h - 1, w, color);
            WriteFastVLine(x, y, h, color);
            WriteFastVLine(x + w - 1, y, h, color);
        }

        private void WriteLine(int x0, int y0, int x1, int y1, Color color)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int dx, dy;
            dx = x1 - x0;
            dy = Math.Abs(y1 - y0);

            int err = dx / 2;
            int ystep;

            if (y0 < y1)
            {
                ystep = 1;
            }
            else
            {
                ystep = -1;
            }

            for (; x0 <= x1; x0++)
            {
                if (steep)
                {
                    DrawPixel(y0, x0, color);
                }
                else
                {
                    DrawPixel(x0, y0, color);
                }
                err -= dy;
                if (err < 0)
                {
                    y0 += ystep;
                    err += dx;
                }
            }
        }
    }
}
