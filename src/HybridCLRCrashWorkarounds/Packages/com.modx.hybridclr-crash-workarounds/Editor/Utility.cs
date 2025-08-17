using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HybridCLR.Editor.CrashWorkarounds
{
    public static class Utility
    {
        public static void ModifySourceFile(this ISymbolOffsetCalculator calculator, string gradleProjectPath)
        {
            var src = "Packages/com.modx.hybridclr-crash-workarounds/Runtime/Source~";
            var dst = Path.Combine(gradleProjectPath, "src/main/Il2CppOutputProject/IL2CPP/libil2cpp/hybridclr");
            var file = "HybridCLRCrashWorkarounds.cpp";

            var text = File.ReadAllText(Path.Combine(src, file));

            var archs = new Architecture[] { Architecture.ARMv7, Architecture.ARM64, Architecture.X86, Architecture.X86_64 };
            foreach (var arch in archs)
            {
                var offset = calculator.GetOffset(arch);
                if (offset != null)
                    text = text.Replace(GetText(arch, 0), GetText(arch, offset.Value));
            }

            File.WriteAllText(Path.Combine(dst, file), text);

            UnityEngine.Debug.Log($"Modified {file}");
        }

        internal static string RunProcess(string fileName, string args)
        {
            return RunProcess(Directory.GetCurrentDirectory(), fileName, args);
        }

        internal static string RunProcess(string workingDirectory, string fileName, string args)
        {
            UnityEngine.Debug.LogFormat("RunProcess {0} {1}", fileName, args);

            using (Process process = new Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.CreateNoWindow = true;

                var output = new StringBuilder();
                process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                    }
                });

                var error = new StringBuilder();
                process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        error.AppendLine(e.Data);
                    }
                });

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception("RunProcess failed: " + error.ToString());

                return output.ToString();
            }
        }

#if !UNITY_2021_1_OR_NEWER
        internal static void Write(this Stream stream, byte[] array)
        {
            stream.Write(array, 0, array.Length);
        }
#endif
        private static string GetText(Architecture architecture, int symbolOffset)
        {
            return $"static int32_t sCreateMonoScriptFromScriptingTypeSymbolOffset = {symbolOffset}; // {architecture}";
        }
    }
}
