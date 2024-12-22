using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GodModeFileExplorer
{
    // PLEASE REWRITE ME IN C++
    // Using c# sucks because of silent errors with File.delete
    // Just use c++
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (!EnablePrivilege(SE_BACKUP_NAME))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fatal Error: Failed to take SeBackupPrivilege.");
                return;
            }
            if (!EnablePrivilege(SE_RESTORE_NAME))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fatal Error: Failed to take SeRestorePrivilege.");
                return;
            }

            args = new string[] { "del", "C:\\Windows\\Sysnative\\ntoskrnl.exe" };

            if (args is null || args.Length <= 0)
            {
                Console.WriteLine("Run \"GMFB.exe /?\" for help.");
                return;
            }

            if (args[0] == "/?")
            {
                Console.WriteLine("GMFB.exe del C:\\File\\To\\Delete.txt");
                Console.WriteLine("GMFB.exe deldir C:\\Folder\\To\\Delete\\");
                Console.WriteLine("GMFB.exe ls C:\\Folder\\To\\Inspect\\");
            }
            else if (args[0] == "del")
            {
                bool wat = File.Exists(args[1]);
                File.Delete(args[1]);
                wat = File.Exists(args[1]);
                Console.WriteLine("Done! Deleted " + args[1]);
            }
            else if (args[0] == "deldir")
            {
                Directory.Delete(args[1], true);
                Console.WriteLine("Done! Deleted " + args[1] + "\\");
            }
            else if (args[0] == "ls")
            {
                foreach(string subdir in Directory.GetDirectories(args[1]))
                {
                    Console.WriteLine(subdir + "\\");
                }
                foreach (string subfile in Directory.GetFiles(args[1]))
                {
                    Console.WriteLine(subfile);
                }
            }
        }
        public static bool EnablePrivilege(string privilegeName)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                // Open the process token
                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out tokenHandle))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Get the LUID for the privilege
                LUID luid;
                if (!LookupPrivilegeValue(null, privilegeName, out luid))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Set up the token privileges structure
                TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Luid = luid,
                    Attributes = SE_PRIVILEGE_ENABLED
                };

                // Adjust the token privileges
                if (!AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                if (Marshal.GetLastWin32Error() != 0)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                return true;
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tokenHandle);
                }
            }
        }
        #region PInvoke Bindings
        // Constants for privileges
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";
        private const string SE_RESTORE_NAME = "SeRestorePrivilege";

        // Constants for token privileges
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const int TOKEN_QUERY = 0x0008;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID Luid;
            public int Attributes;
        }
        #endregion
    }
}