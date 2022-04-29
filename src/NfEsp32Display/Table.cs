namespace NfEsp32Display
{
    public static class Table
    {
        public static string Row(string arg0, string arg1, string arg2, string arg3) => string.Format("|{0,-30}|{1,-17}|{2,4}|{3,6}|", arg0, arg1, arg2, arg3);
    }
}
