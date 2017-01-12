using System.Runtime.InteropServices;

namespace AI_Conquer.BotLibrary.GlobalControls
{
    public enum KeyFlags : byte
    {
        Num0 = 0x30,
        Num1 = 0x31,
        Num2 = 0x32,
        Num3 = 0x33,
        Num4 = 0x34,
        Num5 = 0x35,
        Num6 = 0x36,
        Num7 = 0x37,
        Num8 = 0x38,
        Num9 = 0x39,

        Control = 0x17,
        Escape = 0x1b
    }
    static class KeyBoardSimulator
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        

        public static void KeyDown(KeyFlags key)
        {
            keybd_event(17, 0x45, 0x0001 | 0, 0);
           // keybd_event(17, 0, 0x02, 0);
        }

        public static void KeyUp(KeyFlags key)
        {
            keybd_event(17, 0x45, 0x0001 | 0x0002, 0);
        }
    }
}
