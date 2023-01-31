rule Find_Dynamic_PInvoke_DefinePInvokeMethod
{

    meta:
        description = "Locate use of the DefinePInvokeMethod typebuilder method in .NET binaries or managed code."

    strings:
        $method= "DefinePInvokeMethod"

    condition:
        $method
}