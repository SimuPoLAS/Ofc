namespace Ofc.Util
{
    internal static class NumberHelper
    {
        internal static int NeededBytes(int value)
        {
            if (value < 0) return 4;
            if (value <= 255) return 1;
            if (value <= 65535) return 2;
            return value <= 16777215 ? 3 : 4;
        }

        internal static int NeededBytes(uint value)
        {
            if (value <= 255) return 1;
            if (value <= 65535) return 2;
            return value <= 16777215 ? 3 : 4;
        }
    }
}