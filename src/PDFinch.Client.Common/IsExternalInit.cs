/*
 *  This is a compiler hack for allowing `init` properties for non-NET5.0 targets.
 */

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    // ReSharper disable once UnusedType.Global
    internal static class IsExternalInit { }
}
