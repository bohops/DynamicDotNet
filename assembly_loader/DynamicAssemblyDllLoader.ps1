# Port of DynamicAssemblyDllLoader.cs
# Author: @bohops, @snovvcrash

$assemblyBytes = [System.IO.File]::ReadAllBytes("c:\test\helloworld\bin\x64\Release\helloworld.dll")
$assemblyArgs = [object[]] @("Hello There, World!")

$dynamicMethod = New-Object System.Reflection.Emit.DynamicMethod("_Invoke", [void], @([byte[]], [object], [object[]]))
$iLGenerator = $dynamicMethod.GetiLGenerator()
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Ldarg_0)
$iLGenerator.EmitCall([System.Reflection.Emit.OpCodes]::Call, [System.Reflection.Assembly].GetMethod("Load", [System.Type[]] @([byte[]])), $null)
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Ldstr, "Hello.World")
$iLGenerator.EmitCall([System.Reflection.Emit.OpCodes]::Callvirt, [System.Reflection.Assembly].GetMethod("GetType", [System.Type[]] @([string])), $null)
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Ldstr, "Main")
$iLGenerator.EmitCall([System.Reflection.Emit.OpCodes]::Callvirt, [System.Type].GetMethod("GetMethod", [System.Type[]] @([string])), $null)
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Ldarg_1)
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Ldarg_2)
$iLGenerator.EmitCall([System.Reflection.Emit.OpCodes]::Callvirt, [System.Reflection.MethodBase].GetMethod("Invoke", [System.Type[]] @([object], [object[]])), $null)
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Pop)
$iLGenerator.Emit([System.Reflection.Emit.OpCodes]::Ret)
$dynamicMethod.Invoke($null, [object[]] @($assemblyBytes, $null, (, $assemblyArgs)))
