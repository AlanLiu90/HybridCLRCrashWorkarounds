using System.Collections.Generic;
using HybridCLR.Editor.CrashWorkarounds;
using UnityEditor;
using UnityEngine;

public class MyMonoScriptWriter
{
    [MenuItem("Tools/Generate Mono Scripts")]
    private static void Execute()
    {
        // 需要收集 MonoScript 的热更dll
        var list = new List<string>
        {
            "HotUpdate",
        };

        // 使用反射方式收集 MonoScript
        var monoScriptProvider = MonoScriptProviderFactory.Create(list);

        // 使用dnlib解析方式收集 MonoScript (非 DHE 版)
        // var monoScriptProvider = MonoScriptProviderFactory.Create(list, SettingsUtil.HotUpdateAssemblyNamesExcludePreserved);

        // 使用dnlib解析方式收集 MonoScript (DHE 版)
        // var monoScriptProvider = MonoScriptProviderFactory.Create(list, SettingsUtil.HotUpdateAndDHEAssemblyNamesExcludePreserved);

        // 写入文件（该文件需要热更）
        var output = "Assets/MonoScripts.bytes";
        var writer = new MonoScriptWriter(output, monoScriptProvider);
        writer.Run();

        AssetDatabase.Refresh();

        Debug.Log("Generated MonoScripts.bytes");
    }
}
