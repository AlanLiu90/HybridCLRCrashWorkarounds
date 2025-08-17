using System.Collections.Generic;
using HybridCLR.Editor.CrashWorkarounds;
using UnityEditor;
using UnityEngine;

public class MyMonoScriptWriter
{
    [MenuItem("Tools/Generate Mono Scripts")]
    private static void Execute()
    {
        // ��Ҫ�ռ� MonoScript ���ȸ�dll
        var list = new List<string>
        {
            "HotUpdate",
        };

        // ʹ�÷��䷽ʽ�ռ� MonoScript
        var monoScriptProvider = MonoScriptProviderFactory.Create(list);

        // ʹ��dnlib������ʽ�ռ� MonoScript (�� DHE ��)
        // var monoScriptProvider = MonoScriptProviderFactory.Create(list, SettingsUtil.HotUpdateAssemblyNamesExcludePreserved);

        // ʹ��dnlib������ʽ�ռ� MonoScript (DHE ��)
        // var monoScriptProvider = MonoScriptProviderFactory.Create(list, SettingsUtil.HotUpdateAndDHEAssemblyNamesExcludePreserved);

        // д���ļ������ļ���Ҫ�ȸ���
        var output = "Assets/MonoScripts.bytes";
        var writer = new MonoScriptWriter(output, monoScriptProvider);
        writer.Run();

        AssetDatabase.Refresh();

        Debug.Log("Generated MonoScripts.bytes");
    }
}
