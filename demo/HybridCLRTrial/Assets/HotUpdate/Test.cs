using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using HybridCLR;

[Serializable]
public class SymbolOffsets
{
    public int ARMv7;
    public int ARM64;
}

public class Test : MonoBehaviour
{
    IEnumerator Start()
    {
        // Android: 需要偏移，Unity版本是2020或以上，并且设置为导出Gradle工程时，可以将偏移直接写入C++文件，这里不需要读取文件，否则需要在运行时读取并设置
        // iOS: 不需要偏移
        // OpenHarmony: 需要偏移，并且需要在运行时读取并设置
        // 关于如何记录偏移，请看 SymbolOffsetsWriter.cs

        bool setOffset = false;

#if UNITY_ANDROID && !UNITY_EDITOR && false
        setOffset = true;
#elif UNITY_OPENHARMONY && !UNITY_EDITOR
        setOffset = true;
#endif
        if (setOffset)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "offsets.json");
            var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            string json = request.downloadHandler.text;

            var offsets = JsonUtility.FromJson<SymbolOffsets>(json);

            int offset = 0;
            if (IntPtr.Size == 8)
                offset = offsets.ARM64;
            else
                offset = offsets.ARMv7;

            HybridCLRCrashWorkarounds.SymbolOffset = offset;

            Debug.Log("Offset: " + offset);
        }

        yield return null;

        var ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "monoscripts"));
        var bytes = ab.LoadAsset<TextAsset>("MonoScripts").bytes;
        ab.Unload(true);

        HybridCLRCrashWorkarounds.CreateMonoScripts(bytes);
    }
}
