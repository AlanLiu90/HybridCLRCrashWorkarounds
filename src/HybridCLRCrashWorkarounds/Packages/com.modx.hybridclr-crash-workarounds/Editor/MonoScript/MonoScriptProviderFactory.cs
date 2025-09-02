using System.Collections.Generic;
using System.Linq;
using HybridCLR.Editor.Meta;
using UnityEditor;

namespace HybridCLR.Editor.CrashWorkarounds
{
    public static class MonoScriptProviderFactory
    {
        /// <summary>
        /// 使用dnlib解析方式收集MonoScript
        /// </summary>
        /// <param name="targetDlls">需要收集MonoScript的dll</param>
        /// <param name="allHotUpdateDlls">所有热更dll</param>
        /// <param name="aotDllDir">AOT dll的所在目录，默认为 HybridCLRData/AOTDllOutput/&lt;PLATFORM&gt;</param>
        /// <remarks>
        /// 社区版: 第二个参数传 SettingsUtil.HotUpdateAssemblyNamesExcludePreserved <br />
        /// DHE版: 第二个参数传 SettingsUtil.HotUpdateAndDHEAssemblyNamesExcludePreserved
        /// </remarks>
        public static IMonoScriptProvider Create(IEnumerable<string> targetDlls, IEnumerable<string> allHotUpdateDlls, string aotDllDir = null)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            IAssemblyResolver assemblyResolver;

            if (string.IsNullOrEmpty(aotDllDir))
            {
                assemblyResolver = MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(target, allHotUpdateDlls.ToList());
            }
            else
            {
                assemblyResolver = new CombinedAssemblyResolver(
                    MetaUtil.CreateHotUpdateAssemblyResolver(target, allHotUpdateDlls.ToList()),
                    new PathAssemblyResolver(aotDllDir),
                    new PathAssemblyResolver(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target)));
            }

            var cache = new AssemblyCache(assemblyResolver);
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
