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
        // Unity�汾��2020�����ϣ���������Ϊ����Gradle����ʱ�����Խ�ƫ��ֱ��д��C++�ļ������ﲻ��Ҫ��ȡ�ļ�
        // ʵ�ַ�ʽ�뿴 SymbolOffsetsWriter.cs
#if UNITY_ANDROID && !UNITY_EDITOR && false
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
#endif

        yield return null;

        var ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "monoscripts"));
        var bytes = ab.LoadAsset<TextAsset>("MonoScripts").bytes;
        ab.Unload(true);

        HybridCLRCrashWorkarounds.CreateMonoScripts(bytes);
    }
}
