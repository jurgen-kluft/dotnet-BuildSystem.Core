namespace GameCore
{
    [Flags]
    public enum EPlatform : ulong
    {
        Arch32 = 0,
        Arch64 = 1,
        Mac = 0x400 | Arch64,
        Win32 = 0x800 | Arch32,
        Win64 = 0x800 | Arch64,
        XboxOne = 0x10000 | Arch64,
        XboxOneX = 0x11000 | Arch64,
        XboxSeriesS = 0x12000 | Arch64,
        XboxSeriesX = 0x14000 | Arch64,
        PS4 = 0x20000 | Arch64,
        PS4Pro = 0x21000 | Arch64,
        PS5 = 0x22000 | Arch64,
        NintendoSwitch = 0x40000 | Arch64,
    }
}
