using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;


namespace AI_Conquer.BotLibrary
{
    class LookMemory
    {
        private readonly string _strAddressX;
        private readonly string _strAddressY;
        private readonly Process _process;

        //Constructor
        public LookMemory(string processName, string strAddressX, string strAddressY) // process Name = "conquer"
        {
            _strAddressX = strAddressX;
            _strAddressY = strAddressY;
            _process = Process.GetProcessesByName(processName).FirstOrDefault();
        }

        public Point GetCurrentCoordinate()
        {
            int intAddressX, intAddressY;

            //Check Address is Valid
            if ( int.TryParse(_strAddressX, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out intAddressX)  &&
                 int.TryParse(_strAddressY, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out intAddressY)  )
            {
                int bytesRead;
                
                byte[] bufferXBytes = ReadMemory(_process, intAddressX, 4, out bytesRead);
                byte[] bufferYBytes = ReadMemory(_process, intAddressY, 4, out bytesRead);
                return new Point(BitConverter.ToInt32(bufferXBytes, 0), BitConverter.ToInt32(bufferYBytes, 0)); 
            }
            return Point.Empty;
        }

        public static byte[] ReadMemory(Process process, int address, int numOfBytes, out int bytesRead)
        {
            IntPtr hProc = OpenProcess(ProcessAccessFlags.VMRead, false, process.Id);
            var buffer = new byte[numOfBytes];
            ReadProcessMemory(hProc, new IntPtr(address), buffer, numOfBytes, out bytesRead);
            return buffer;
        }

        #region WIN32 API
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010, //Read Memory
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);
        #endregion
    }
}
