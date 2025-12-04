namespace MageSim.Integration.Input
{
    public static class KeyMap
    {
        public static byte Map(string key)
        {
            if (key.Length == 2 && key[0] == 'D' && char.IsDigit(key[1]))
                return (byte)('0' + (key[1] - '0')); // D1..D9 → '1'..'9'

            switch (key.ToUpperInvariant())
            {
                case "Q": return 0x51;
                case "W": return 0x57;
                case "E": return 0x45;
                case "R": return 0x52;
                case "SPACE": return 0x20;
                default: return 0x20;
            }
        }
    }
}
