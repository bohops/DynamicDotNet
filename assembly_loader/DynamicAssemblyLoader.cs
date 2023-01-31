// DynamicAssemblyLoader: A DotNet Assembly Loader using a Dynamic Method and Emitted MSIL Instructions
// Author: @bohops
//
// "Normal" Implementation:
/*
 Assembly assembly = Assembly.Load(assemblyBytes);
 assembly.EntryPoint.Invoke(obj, objArr);   
*/

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

public class Program
{
    public static void Main()
    {
        //Capture assembly bytes from memory, disk, etc.
        //Example - Run Seatbelt by @harmj0y and @tifkin_ [https://github.com/GhostPack/Seatbelt]
        byte[] assemblyBytes = File.ReadAllBytes(@"C:\test\Seatbelt.exe");

        //Args in string array format { "str1", "str2", "etc"}
        string[] assemblyArgs = new string[] { "-group=system" };
        object obj = new object();
        object[] objArr = new object[] { assemblyArgs };

        //Load and invoke the assembly
        DynamicMethod dynamicMethod = new DynamicMethod("_Invoke", typeof(void), new Type[] { typeof(byte[]), typeof(object), typeof(object[]) });
        ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.EmitCall(OpCodes.Call, typeof(Assembly).GetMethod("Load", new Type[] { typeof(byte[]) }), null);
        iLGenerator.EmitCall(OpCodes.Callvirt, typeof(Assembly).GetMethod("get_EntryPoint", new Type[] { }), null);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldarg_2);
        iLGenerator.EmitCall(OpCodes.Callvirt, typeof(MethodBase).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) }), null);
        iLGenerator.Emit(OpCodes.Pop);
        iLGenerator.Emit(OpCodes.Ret);
        dynamicMethod.Invoke(null, new object[] { assemblyBytes, obj, objArr });
    }
}