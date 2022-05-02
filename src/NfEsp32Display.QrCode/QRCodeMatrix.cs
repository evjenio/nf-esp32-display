using System.Collections;
using System.Collections.Generic;

namespace NfEsp32Display.QrCode
{
    internal sealed class QRCodeMatrix
    {
        public QRCodeMatrix(int version)
        {
            int size = 21 + (version - 1) * 4;
            this.ModuleMatrix = new List<BitArray>();
            for (var i = 0; i < size; i++)
                this.ModuleMatrix.Add(new BitArray(size));
        }

        public bool GetValue(int x, int y) => ModuleMatrix[x][y];

        public List<BitArray> ModuleMatrix { get; set; }
    }
}
