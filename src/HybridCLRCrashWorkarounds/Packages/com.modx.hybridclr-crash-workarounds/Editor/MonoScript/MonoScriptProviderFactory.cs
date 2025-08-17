using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using HybridCLR.Editor.Meta;

namespace HybridCLR.Editor.CrashWorkarounds
{
    public static class MonoScriptProviderFactory
    {
        /// <summary>
        /// ʹ��dnlib������ʽ�ռ�MonoScript
        /// </summary>
        /// <param name="targetDlls">��Ҫ�ռ�MonoScript��dll</param>
        /// <param name="allHotUpdateDlls">�����ȸ�dll</param>
        /// <remarks>
        /// ������: �ڶ��������� SettingsUtil.HotUpdateAssemblyNamesExcludePreserved
        /// DHE��: �ڶ��������� SettingsUtil.HotUpdateAndDHEAssemblyNamesExcludePreserved
        /// </remarks>
        public static IMonoScriptProvider Create(IEnumerable<string> targetDlls, IEnumerable<string> allHotUpdateDlls)
        {
            var cache = new AssemblyCache(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(EditorUserBuildSettings.activeBuildTarget, allHotUpdateDlls.ToList()));
            var analyzer = new MonoScriptAnalyzer(cache, targetDlls);
            analyzer.Run();

            return analyzer;
        }

        /// <summary>
        /// ʹ�÷��䷽ʽ�ռ�MonoScript
        /// </summary>
        /// <param name="targetDlls">��Ҫ�ռ�MonoScript��dll</param>
        public static IMonoScriptProvider Create(IEnumerable<string> targetDlls)
        {
            var reflector = new MonoScriptReflector(targetDlls);
            reflector.Run();

            return reflector;
        }
    }
}
