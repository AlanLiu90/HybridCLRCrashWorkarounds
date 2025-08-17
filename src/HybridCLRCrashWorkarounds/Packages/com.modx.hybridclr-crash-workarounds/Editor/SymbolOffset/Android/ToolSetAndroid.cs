using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.CrashWorkarounds
{
    internal sealed class ToolSetAndroid
    {
        public string ObjDump { get; private set; }

        public ToolSetAndroid(Architecture architecture)
        {
            ObjDump = GetTool("objdump", architecture);
        }

        private static string GetTool(string tool, Architecture architecture)
        {
            string toolDir = EditorApplication.applicationContentsPath;

            if (Application.platform == RuntimePlatform.OSXEditor)
                toolDir = Path.Combine(toolDir, "../..");

            string llvmArch;
            if (Application.platform == RuntimePlatform.WindowsEditor)
                llvmArch = "windows-x86_64";
            else if (Application.platform == RuntimePlatform.OSXEditor)
                llvmArch = "darwin-x86_64";
            else
                llvmArch = "linux-x86_64";

            toolDir = Path.Combine(toolDir, $"PlaybackEngines/AndroidPlayer/NDK/toolchains/llvm/prebuilt/{llvmArch}/bin");

            string prefix;

            switch (architecture)
            {
                case Architecture.ARMv7:
                    prefix = "arm-linux-androideabi-";
                    break;

                case Architecture.ARM64:
                    prefix = "aarch64-linux-android-";
                    break;

                case Architecture.X86:
                    prefix = "i686-linux-android-";
                    break;

                case Architecture.X86_64:
                    prefix = "x86_64-linux-android-";
                    break;

                default:
                    throw new NotSupportedException(architecture.ToString());
            }

            var toolPath = Path.Combine(toolDir, prefix + tool);
            if (Application.platform == RuntimePlatform.WindowsEditor)
                toolPath += ".exe";

            if (!File.Exists(toolPath))
            {
                toolPath = Path.Combine(toolDir, GetLLVMTool(tool));
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    toolPath += ".exe";

                if (!File.Exists(toolPath))
                    throw new Exception($"Failed to find '{tool}'");
            }
            
            return toolPath;
        }

        private static string GetLLVMTool(string tool)
        {
            return "llvm-" + tool;
        }
    }
}
