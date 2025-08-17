using System;
using System.IO;
using System.Linq;
using System.Text;

namespace HybridCLR.Editor.CrashWorkarounds
{
    public sealed class MonoScriptWriter
    {
        public delegate bool Filter(string assemblyName, string nameSpace, string className);

        private readonly string mOutputPath;
        private readonly IMonoScriptProvider mMonoScriptProvider;
        private readonly Filter mFilter;

        public MonoScriptWriter(string outputPath, IMonoScriptProvider monoScriptProvider, Filter filter = null)
        {
            mOutputPath = outputPath;
            mMonoScriptProvider = monoScriptProvider;
            mFilter = filter;
        }

        public void Run()
        {
            using (var stream = File.Open(mOutputPath, FileMode.Create, FileAccess.Write))
            {
                var monoScripts = mMonoScriptProvider.MonoScripts;

                stream.Write(Encoding.ASCII.GetBytes("UMSB"));

                int assemblyCount = monoScripts.Count;
                stream.Write(BitConverter.GetBytes(assemblyCount));

                foreach (var kv in monoScripts.OrderBy(x => x.Key))
                {
                    var assemblyName = kv.Key;
                    WriteString(stream, assemblyName);

                    int nameSpaceCount = kv.Value.Count;
                    stream.Write(BitConverter.GetBytes(nameSpaceCount));

                    foreach (var kv2 in kv.Value.OrderBy(x => x.Key))
                    {
                        var nameSpace = kv2.Key;
                        WriteString(stream, nameSpace);

                        var classNames = kv2.Value;
                        if (mFilter != null)
                            classNames = classNames.Where(x => mFilter(assemblyName, nameSpace, x)).ToList();

                        int classNameCount = classNames.Count;
                        stream.Write(BitConverter.GetBytes(classNameCount));

                        foreach (var className in classNames.OrderBy(x => x))
                        {
                            WriteString(stream, className);
                        }
                    }
                }
            }
        }

        private void WriteString(FileStream stream, string s)
        {
            var buffer = Encoding.UTF8.GetBytes(s);
            var buffer2 = new byte[buffer.Length + 1];
            Array.Copy(buffer, buffer2, buffer.Length);
            buffer2[buffer.Length] = 0;

            stream.Write(BitConverter.GetBytes(buffer2.Length));
            stream.Write(buffer2);
        }
    }
}
