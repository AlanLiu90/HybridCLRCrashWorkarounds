using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using HybridCLR.Editor.Meta;

namespace HybridCLR.Editor.CrashWorkarounds
{
    public static class MonoScriptProviderFactory
    {
        /// <summary>
        /// 使用dnlib解析方式收集MonoScript
        /// </summary>
        /// <param name="targetDlls">需要收集MonoScript的dll</param>
        /// <param name="allHotUpdateDlls">所有热更dll</param>
        /// <remarks>
        /// 社区版: 第二个参数传 SettingsUtil.HotUpdateAssemblyNamesExcludePreserved
        /// DHE版: 第二个参数传 SettingsUtil.HotUpdateAndDHEAssemblyNamesExcludePreserved
        /// </remarks>
        public static IMonoScriptProvider Create(IEnumerable<string> targetDlls, IEnumerable<string> allHotUpdateDlls)
        {
            var cache = new AssemblyCache(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(EditorUserBuildSettings.activeBuildTarget, allHotUpdateDlls.ToList()));
            var analyzer = new MonoScriptAnalyzer(cache, targetDlls);
            analyzer.Run();

            return analyzer;
        }

        /// <summary>
        /// 使用反射方式收集MonoScript
        /// </summary>
        /// <param name="targetDlls">需要收集MonoScript的dll</param>
        public static IMonoScriptProvider Create(IEnumerable<string> targetDlls)
        {
            var reflector = new MonoScriptReflector(targetDlls);
            reflector.Run();

            return reflector;
        }
    }
}
