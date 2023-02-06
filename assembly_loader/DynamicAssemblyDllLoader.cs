// DynamicAssemblyDllLoader: A DotNet Assembly DLL Loader using a Dynamic Method and Emitted MSIL Instructions
// Author: @bohops
//
// ------------------------------------------------------------------------
//
// helloworld.dll source - compile to .NET Framework maanged DLL for testing
/*
using System;

namespace Hello
{
    public class World
    {
        public static void Run(string[] args)
        {
            Console.WriteLine(args[0]);
        }
    }
}
*/
//
// ------------------------------------------------------------------------
//
// "Normal" Assembly DLL Load Implementation:
/*
 byte[] assemblyBytes = File.ReadAllBytes(..)
 Assembly assembly = Assembly.Load(assemblyBytes);
 Type type = assembly.GetType("Hello.World");
 MethodInfo method = type.GetMethod("Run");
 method.Invoke(obj, objArr); 
*/
// ------------------------------------------------------------------------
//
// "Dyanmic" Assembly DLL Load Implementation:

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

public class Program
{
    public static void Main()
    {
        //Capture assembly bytes from memory, disk, etc.
        byte[] assemblyBytes = File.ReadAllBytes(@"c:\test\helloworld\bin\x64\Release\helloworld.dll");

        //Args in string array format { "str1", "str2", "etc"}
        string[] assemblyArgs = new string[] { "Hello There, World!" };
        object obj = new object();
        object[] objArr = new object[] { assemblyArgs };

        //Execute in dynamic function
        DynamicMethod dynamicMethod = new DynamicMethod("_Invoke", typeof(void), new Type[] { typeof(byte[]), typeof(object), typeof(object[]) });
        ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.EmitCall(OpCodes.Call, typeof(Assembly).GetMethod("Load", new Type[] { typeof(byte[]) }), null);
        iLGenerator.Emit(OpCodes.Ldstr, "Hello.World"); //Type (e.g. Namespace/Class)
        iLGenerator.EmitCall(OpCodes.Callvirt, typeof(Assembly).GetMethod("GetType", new Type[] { typeof(string) }), null);
        iLGenerator.Emit(OpCodes.Ldstr, "Run"); //Method Name
        iLGenerator.EmitCall(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string) }), null);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldarg_2);
        iLGenerator.EmitCall(OpCodes.Callvirt, typeof(MethodBase).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) }), null);
        iLGenerator.Emit(OpCodes.Pop);
        iLGenerator.Emit(OpCodes.Ret);
        dynamicMethod.Invoke(null, new object[] { assemblyBytes, obj, objArr });
    }
}