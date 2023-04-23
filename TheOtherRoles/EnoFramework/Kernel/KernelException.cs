using System;

namespace TheOtherRoles.EnoFramework.Kernel;

public class KernelException : Exception
{
    public KernelException(string? message = null)
    {
        System.Console.WriteLine(message);
    }
}