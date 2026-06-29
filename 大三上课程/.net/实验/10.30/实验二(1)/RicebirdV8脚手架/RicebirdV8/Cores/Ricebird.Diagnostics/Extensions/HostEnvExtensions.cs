using System.Runtime.CompilerServices;

namespace Ricebird.Framework
{
    internal static class HostEnvExtensions
    {

        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static bool ShouldLog(this HostEnv env) => env.FrameworkOptions.DiagnosticsDatabase != null;
    }
}
