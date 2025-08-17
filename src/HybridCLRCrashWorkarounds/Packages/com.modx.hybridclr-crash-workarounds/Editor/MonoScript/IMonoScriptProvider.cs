using System.Collections.Generic;

namespace HybridCLR.Editor.CrashWorkarounds
{
    public interface IMonoScriptProvider
    {
        Dictionary<string, Dictionary<string, List<string>>> MonoScripts { get; }
    }
}
