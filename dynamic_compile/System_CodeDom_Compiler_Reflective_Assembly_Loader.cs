//mitre att&ck: T1027.004 - Obfuscated Files or Information: Compile After Delivery [https://attack.mitre.org/techniques/T1027/004/]
//ref: https://github.com/MarianoIT/dynamic-code/blob/0e190f73931478ffa7f1c265fd3efa1f48eff41c/DynamicCodeApp/Program.cs

using System;
using System.Reflection;
using System.CodeDom.Compiler;

public class App
{
    public static void Main()
    {
        // Insert source code to compile and run
        string code = @"
using System;
public class myclass
{
    public static void Main(string[] args)
    {
        Console.WriteLine(""It worked!"");
    }
}";
        //Specify coding language
        CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
        
        //Add assembly references
        CompilerParameters compilerParameters = new CompilerParameters();
        compilerParameters.ReferencedAssemblies.Add("System.dll");

        //Specify code generation options (needed to load in memory)
        compilerParameters.GenerateInMemory = true;
        compilerParameters.GenerateExecutable = true;

        //Compile code
        CompilerResults results = codeDomProvider.CompileAssemblyFromSource(compilerParameters, new String[] { code });

        //Reflectively load assembly (with args)
        Assembly assembly = results.CompiledAssembly;
        string[] args = new string[] { "" };
        assembly.EntryPoint.Invoke(null, new object[] { args });
    }
}
