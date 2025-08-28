#if TUANJIE_1_0_OR_NEWER

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.CrashWorkarounds
{
    internal sealed class ToolSetOpenHarmony
    {
        public string ObjDump { get; private set; }

        public ToolSetOpenHarmony(Architecture architecture)
        {
            var toolDir = GetToolDir();

            ObjDump = GetTool("llvm-objdump", toolDir);
        }

        private static string GetToolDir()
        {
            string toolDir = EditorApplication.applicationContentsPath;

            if (Application.platform == RuntimePlatform.OSXEditor)
                toolDir = Path.Combine(toolDir, "../..");

            toolDir = Path.Combine(toolDir, $"PlaybackEngines/OpenHarmonyPlayer/SDK");

            toolDir = Directory.GetDirectories(toolDir)
                .Where(x => int.TryParse(Path.GetFileName(x), out _))
                .OrderBy(x => Path.GetFileName(x))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(toolDir))
                throw new Exception("Failed to find valid sdk directory");

            toolDir = Path.Combine(toolDir, "native/llvm/bin");
            return toolDir;
        }

        private static string GetTool(string tool, string toolDir)
        {
            var toolPath = Path.Combine(toolDir, tool);
            if (Application.platform == RuntimePlatform.WindowsEditor)
                toolPath += ".exe";

            if (!File.Exists(toolPath))
                throw new Exception($"Failed to find '{tool}'");

            return toolPath;
        }
    }
}

#endif
