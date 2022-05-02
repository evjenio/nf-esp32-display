namespace NfEsp32Display.QrCode
{
    public sealed class QRCodeData
    {
        public QRCodeData(string text)
        {
            Text = text;
            EccLevel = EccLevel.Q;
        }

        public EccLevel EccLevel { get; set; }

        internal string Text { get; }
    }
}
