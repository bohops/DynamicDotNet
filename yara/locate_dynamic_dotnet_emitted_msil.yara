rule Find_Dynamic_DotNet_Emitted_MSIL
{
    meta:
        description = "Identify use of emitted MSIL instructions in .NET binaries or managed code."

    strings:
        $a= "GetILGenerator"
        $b = "Emit"
        $c = "OpCodes"

    condition:
        $a and $b and $c
}