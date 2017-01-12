using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace AI_Conquer.BotLibrary.GlobalControls
{

     class MouseSimulator
    {
         

         public static void Simulate(MouseFlags commandFlags)//Point position)
         {
             mouse_event((uint)commandFlags, 0, 0, 0, 0);
         }
         public static int MakeLParam(Point ptr)
         {
             return ((ptr.Y << 16) | (ptr.X & 0xffff));
         }

         public static bool Jump(Point clickPoint)
         {
             var childHandle = IntPtr.Zero;
             if (!GetHandle(ref childHandle)) return false;
             int lParam = MakeLParam(clickPoint);
             //SendMessage(childHandle, (uint)WindowMessages.WM_SETCURSOR, childHandle, (IntPtr)0x02000001);
             //IntPtr child_child = GetWindow(childHandle, GetWindow_Cmd.GW_CHILD);
             
            // PostMessage((IntPtr)0x00D057C, (int)WindowMessages.WM_KEYDOWN, (int)0x11, (int)0x001D0001);
            // Thread.Sleep(300);
             PostMessage(childHandle,(uint)WindowMessages.WM_MOUSEMOVE,(int)MouseKeyboard.MK_CONTROL,lParam) ;
             PostMessage(childHandle, (uint)WindowMessages.WM_LBUTTONDOWN, (int)(MouseKeyboard.MK_LBUTTON | MouseKeyboard.MK_CONTROL), lParam);
             PostMessage(childHandle, (uint) WindowMessages.WM_MOUSEMOVE, (int) MouseKeyboard.MK_CONTROL, lParam);
             PostMessage(childHandle, (uint)WindowMessages.WM_LBUTTONUP, (int)MouseKeyboard.MK_CONTROL, lParam);
             return true;
         }

         public static bool LeftClick(Point clickPoint)
         {
             var childHandle = IntPtr.Zero;
             if (!GetHandle(ref childHandle)) return false;
             int lParam = MakeLParam(clickPoint);
             PostMessage(childHandle, (uint)WindowMessages.WM_LBUTTONDOWN, (int)MouseKeyboard.MK_LBUTTON, lParam);
             PostMessage(childHandle, (uint)WindowMessages.WM_LBUTTONUP, 0, lParam);
             return true;
         }

         public static bool RightClick(Point clickPoint)
         {
             var childHandle = IntPtr.Zero;
             if (!GetHandle(ref childHandle)) return false;
             int lParam = MakeLParam(clickPoint);
             PostMessage(childHandle, (uint)WindowMessages.WM_RBUTTONDOWN, (int)MouseKeyboard.MK_RBUTTON, lParam);
             PostMessage(childHandle, (uint)WindowMessages.WM_RBUTTONUP, 0, lParam);
             return true;
         }

         private static bool GetHandle(ref IntPtr child)
         {
             IntPtr parent = FindWindow(null, "[Conquer] Invasion Of Pirates");
             if (parent == IntPtr.Zero)
                 return false;
             child = GetWindow(parent, GetWindow_Cmd.GW_CHILD);
             if (child == IntPtr.Zero)
                 return false;
             return true;
         }

         public static bool GetWindowRectangle(ref RECT rct)
         {
             var childHandle = IntPtr.Zero;
             if (!GetHandle(ref childHandle)) return false;
             rct = new RECT();
             GetWindowRect(childHandle, ref rct);
             return true;
         }

         #region WIN32 API
         [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        
         [DllImport("user32.dll", EntryPoint = "WindowFromPoint",
           CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
#endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

         [Flags]
         enum MouseKeyboard : uint
        {
            MK_LBUTTON = 0x0001,
            MK_RBUTTON = 0x0002,
            MK_CONTROL = 0x0008
        }

        enum WindowMessages : uint
        {

            WM_MOUSEACTIVATE = 0x0021,
            WM_SETCURSOR = 0x0020,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205


        }

        enum HitTestCodes : uint
        {
            HTCLIENT = 1,
        }
        
        public enum MouseFlags : uint
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x00000800,
            XDown = 0x00000080,
            XUp = 0x00000100
        }
    }
}
