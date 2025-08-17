using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.CrashWorkarounds
{
    internal sealed class SymbolOffsetCalculatorAndroid : ISymbolOffsetCalculator
    {
        private static readonly Dictionary<Architecture, string> mArchs = new Dictionary<Architecture, string>()
        {
            [Architecture.ARMv7] = "armeabi-v7a",
            [Architecture.ARM64] = "arm64-v8a",
            [Architecture.X86] = "x86",
            [Architecture.X86_64] = "x86_64",
        };

        private readonly string mTargetSymbolPattern;
        private readonly string mBaseSymbolPattern;
        private readonly string mGradleProjectDir;
        private bool mDevelopmentBuild;
        private readonly Dictionary<Architecture, int> mOffsets = new Dictionary<Architecture, int>();

        public SymbolOffsetCalculatorAndroid(string targetSymbolPattern, string baseSymbolPattern, string gradleProjectDir, bool developmentBuild)
        {
            mTargetSymbolPattern = targetSymbolPattern;
            mBaseSymbolPattern = baseSymbolPattern;
            mGradleProjectDir = gradleProjectDir;
            mDevelopmentBuild = developmentBuild;
        }

        public int? GetOffset(Architecture architecture)
        {
            if (mOffsets.TryGetValue(architecture, out int offset))
                return offset;
            else
                return null;
        }

        public void Run()
        {
            foreach (var kv in mArchs)
            {
                var arch = kv.Value;

                var toolset = new ToolSetAndroid(kv.Key);

                var libUnityPath = Path.Combine(mGradleProjectDir, $"src/main/jniLibs/{arch}/{GetLibUnity()}.so");

                if (!File.Exists(libUnityPath))
                    continue;

                var symbolFile = GetLibUnitySymbol(arch);
                var symbolText = Utility.RunProcess(toolset.ObjDump, $"--syms \"{symbolFile}\"");

                int targetSymbolAddress = GetSymbolAddress(mTargetSymbolPattern, symbolText);
                int baseSymbolAddress = GetSymbolAddress(mBaseSymbolPattern, symbolText);

                mOffsets.Add(kv.Key, targetSymbolAddress - baseSymbolAddress);

                Debug.Log($"Calculated the offset for {arch}");
            }
        }

        private string GetLibUnitySymbol(string arch)
        {
            string path;
            var libUnity = GetLibUnity();

            if (PlayerSettings.stripEngineCode)
            {
#if UNITY_2021_1_OR_NEWER
                path = $"Library/Bee/artifacts/Android/{libUnity}/{arch}/{libUnity}.sym.so";
#else
                path = $"Temp/StagingArea/symbols/{arch}/{libUnity}.sym.so";
#endif
            }
            else
            {
                path = EditorApplication.applicationContentsPath;

                if (Application.platform == RuntimePlatform.OSXEditor)
                    path = Path.Combine(path, "../..");

                string type = mDevelopmentBuild ? "Development" : "Release";
                path = Path.Combine(path, $"PlaybackEngines/AndroidPlayer/Variations/il2cpp/{type}/Symbols/{arch}/{libUnity}.sym.so");
            }

            return path;
        }

        private static string GetLibUnity()
        {
#if TUANJIE_1_0_OR_NEWER
            return "libtuanjie";
#else
            return "libunity";
#endif
        }

        private static int GetSymbolAddress(string pattern, string symbolText)
        {
            var regex = new Regex(pattern);
            var matches = regex.Matches(symbolText);

            if (matches.Count != 1)
                throw new Exception($"Invalid matched count of '{pattern}'. Expected: 1, Actual: {matches.Count}");

            string matchedLine = matches[0].Groups[0].Value.Trim();
            string address = matchedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First();
            return int.Parse(address, NumberStyles.HexNumber);
        }
    }
}
