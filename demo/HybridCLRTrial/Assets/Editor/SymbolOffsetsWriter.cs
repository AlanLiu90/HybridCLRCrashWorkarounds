using System.IO;
using HybridCLR.Editor.CrashWorkarounds;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal sealed class SymbolOffsetsWriter : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
#if TUANJIE_1_0_OR_NEWER
    , UnityEditor.OpenHarmony.IPostGenerateOpenHarmonyProject
#endif
{
    public int callbackOrder => 100;

    private static bool mDevelopmentBuild;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
            return;

        mDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
    }

    public void OnPostGenerateGradleAndroidProject(string outputPath)
    {
        var calculator = SymbolOffsetCalculatorFactory.CreateForAndroid(outputPath, mDevelopmentBuild);
        calculator.Run();

        bool writeToSourceFile = false;

#if UNITY_2020_1_OR_NEWER
        writeToSourceFile = EditorUserBuildSettings.exportAsGoogleAndroidProject;
#endif

        // Unity�汾��2020�����ϣ���������Ϊ����Gradle����ʱ�����Խ�ƫ��ֱ��д��C++�ļ�������Ҫ����һ���ļ�������ʱҲ����Ҫ���⴦��
        if (writeToSourceFile)
            calculator.ModifySourceFile(outputPath);
        else
            WriteJSON(calculator, Path.Combine(outputPath, $"src/main/assets/offsets.json"));
    }

#if TUANJIE_1_0_OR_NEWER
    public void OnPostGenerateOpenHarmonyProject(string outputPath)
    {
        // ����ִ�е�����ʱ��libil2cpp.so�Ѿ������ˣ��޷���Android�����޸�C++�ļ���ֻ�ܽ�ƫ�Ƽ�¼���ļ���

        var calculator = SymbolOffsetCalculatorFactory.CreateForOpenHarmony(outputPath, mDevelopmentBuild);
        calculator.Run();

        WriteJSON(calculator, Path.Combine(outputPath, "src/main/resources/rawfile/Data/StreamingAssets/offsets.json"));
    }
#endif

    private void WriteJSON(ISymbolOffsetCalculator calculator, string outputPath)
    {
        var offsets = new SymbolOffsets();

        int? offset = calculator.GetOffset(Architecture.ARM64);
        if (offset != null)
            offsets.ARM64 = offset.Value;

        offset = calculator.GetOffset(Architecture.ARMv7);
        if (offset != null)
            offsets.ARMv7 = offset.Value;

        var json = JsonUtility.ToJson(offsets);
        File.WriteAllText(outputPath, json);

        Debug.Log("Wrote offsets.json");
    }
}
