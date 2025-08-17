using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HybridCLR.Editor.CrashWorkarounds
{
    internal sealed class CopyFile : IPreprocessBuildWithReport
    {
        public int callbackOrder => 100;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android ||
                report.summary.platform == BuildTarget.iOS)
            {
                var src = "Packages/com.modx.hybridclr-crash-workarounds/Runtime/Source~";
                var dst = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp/hybridclr");
                var file = "HybridCLRCrashWorkarounds.cpp";

                File.Copy(Path.Combine(src, file), Path.Combine(dst, file), true);

                Debug.Log("Copied HybridCLRCrashWorkarounds.cpp");
            }
        }
    }
}
