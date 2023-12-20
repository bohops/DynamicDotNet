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

namespace DynamicUsageLogPatch
{
    public class Program
    {
        public static void Main()
        {
            IntPtr kernel32 = LoadLibrary("KernelBase.dll");

            IntPtr createFileW = GetProcAddress(kernel32, "CreateFileW");

            UInt32 dwOld = 0;
            VirtualProtect(createFileW, (UInt32)0x1, 0x40, ref dwOld);

            byte[] patch = new byte[] { 0xC3 };
            
            //Do before process exit
            Marshal.Copy(patch, 0, createFileW, patch.Length);

            VirtualProtect(createFileW, (UInt32)0x1, 0x20, ref dwOld);
        }

        public static object DynamicPInvokeBuilder(Type type, string library, string method, Object[] args, Type[] paramTypes)
        {
            AssemblyName assemblyName = new AssemblyName("zz");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("zz", false);
            var typeBuilder = moduleBuilder.DefineType(library, TypeAttributes.Public | TypeAttributes.Class);

            var methodBuilder = typeBuilder.DefineMethod(method,
                                                         MethodAttributes.Public | MethodAttributes.Static,
                                                         type,
                                                         paramTypes);

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
                                              CharSet.Ansi};

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(dllImportConstructorInfo, new Object[] { library }, dllImportFieldInfo, dllImportFieldValues);
            methodBuilder.SetCustomAttribute(customAttributeBuilder);

            Type myType = typeBuilder.CreateType();
            object res = myType.InvokeMember(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, args);

            return res;
        }

        public static IntPtr LoadLibrary(String libName)
        {
            Type[] paramTypes = { typeof(String) };
            Object[] args = { libName };
            object res = DynamicPInvokeBuilder(typeof(IntPtr), "kernel32.dll", "LoadLibraryA", args, paramTypes);
            return (IntPtr)res;
        }

        public static IntPtr GetProcAddress(IntPtr hModule, String procName)
        {
            Type[] paramTypes = { typeof(IntPtr), typeof(String) };
            Object[] args = { hModule, procName };
            object res = DynamicPInvokeBuilder(typeof(IntPtr), "kernel32.dll", "GetProcAddress", args, paramTypes);
            return (IntPtr)res;
        }

        public static bool VirtualProtect(IntPtr hProcess, UInt32 dwSize, UInt32 flNewProtect, ref UInt32 lpflOldProtect)
        {
            Type[] paramTypes = { typeof(IntPtr), typeof(UInt32), typeof(UInt32), typeof(UInt32).MakeByRefType() };
            Object[] args = { hProcess, dwSize, flNewProtect, lpflOldProtect };
            object res = DynamicPInvokeBuilder(typeof(bool), "kernel32.dll", "VirtualProtect", args, paramTypes);
            return (bool)res;
        }
    }
}