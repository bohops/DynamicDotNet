rule Find_Dynamic_PInvoke_DefineMethod
{

    meta:
        description = "Locate use of the DefineMethod typebuilder method and interesting strings in .NET binaries or managed code."

    strings:
        $a = "DefineMethod"
        $b = "VirtualProtect"

    condition:
        $a and $b
}