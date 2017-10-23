using System;
using System.Globalization;

[Flags]
public enum NoiseFlags
{
    None = 0,
    FlipStripes = 1 << 0,
    ModPattern1 = 1 << 1,
    ModPattern2 = 1 << 2,
    ModPattern3 = 1 << 3,
    ModPattern4 = 1 << 4,
    ModPattern5 = 1 << 5,
    ModPattern6 = 1 << 6,
    ModPattern7 = 1 << 7,
    ModPattern8 = 1 << 8,
    ModPattern9 = 1 << 9,
    ModPattern10 = 1 << 10,
    ModPattern11 = 1 << 11,
    ModPattern12 = 1 << 12,
    ModPattern13 = 1 << 13,
    ModPattern14 = 1 << 14,
    ModPattern15 = 1 << 15,
    ModPattern16 = 1 << 16,
    Glass1 = 1 << 17,
    Glass2 = 1 << 18,
    Glass3 = 1 << 19,
    Glass4 = 1 << 20,
    FreakyFriday = 1 << 21,
    TigerStripes = 1 << 22,
    ReverseHollow = 1 << 23,
    Solid = 1 << 24,
    Patterned = 1 << 25,
    Striped = 1 << 26,
    Unused = 1 << 27,
    Islands1 = 1 << 28,
    Islands2 = 1 << 29,
    Islands3 = 1 << 30,
    Islands4 = 1 << 31
}

static class Flags
{
    static NoiseFlags _flags = NoiseFlags.None;

    public static void Set(NoiseFlags mask, bool val)
    {
        if (val)
        {
            Game.LogAppend(mask.ToString());
            _flags |= mask;
        }
        else
        {
            _flags &= ~mask;
        }
    }

    public static void Set(string flag, bool val)
    {
        NoiseFlags mask;
        if (Enum.TryParse(flag, out mask))
        {
            Set(mask, val);
        }
    }

    public static bool Get(NoiseFlags mask)
    {
        return _flags.HasFlag(mask);
    }

    public static bool Get(string flag)
    {
        NoiseFlags mask;
        if (Enum.TryParse(flag, out mask))
        {
            return Get(mask);
        }

        return false;
    }

    public static string ToHex()
    {
        return _flags.ToString("X");
    }

    public static void FromHex(string hex)
    {
        int bitmask;
        if (Int32.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bitmask))
        {
            _flags = (NoiseFlags)bitmask;
        }
    }
}
