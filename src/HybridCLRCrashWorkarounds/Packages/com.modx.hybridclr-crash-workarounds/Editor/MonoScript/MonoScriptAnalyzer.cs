using System.Collections.Generic;
using System.IO;
using dnlib.DotNet;
using HybridCLR.Editor.Meta;

namespace HybridCLR.Editor.CrashWorkarounds
{
    internal sealed class MonoScriptAnalyzer : IMonoScriptProvider
    {
        public Dictionary<string, Dictionary<string, List<string>>> MonoScripts => mMonoScripts;

        private static readonly string mMonoBehaviourTypeName = "UnityEngine.MonoBehaviour";
        private static readonly string mScriptableObjectTypeName = "UnityEngine.ScriptableObject";

        private readonly List<ModuleDefMD> mRootModules = new List<ModuleDefMD>();
        private readonly Dictionary<string, Dictionary<string, List<string>>> mMonoScripts = new Dictionary<string, Dictionary<string, List<string>>>();

        public MonoScriptAnalyzer(AssemblyCache cache, IEnumerable<string> assemblyNames)
        {
            foreach (var assemblyName in assemblyNames)
            {
                mRootModules.Add(cache.LoadModule(assemblyName));
            }
        }

        public void Run()
        {
            foreach (var module in mRootModules)
            {
                var monoScripts = new Dictionary<string, List<string>>();

                foreach (var type in module.GetTypes())
                {
                    if (type.BaseType == null)
                        continue;

                    if (type.IsAbstract)
                        continue;

                    if (type.GenericParameters.Count != 0)
                        continue;

                    if (type.DeclaringType != null)
                        continue;

                    if (InheritsFrom(type, mMonoBehaviourTypeName) || InheritsFrom(type, mScriptableObjectTypeName))
                    {
                        var ns = type.Namespace.ToString();
                        if (!monoScripts.TryGetValue(ns, out var list))
                        {
                            list = new List<string>();
                            monoScripts.Add(ns, list);
                        }

                        list.Add(type.Name);
                    }
                }

                if (monoScripts.Count > 0)
                    mMonoScripts.Add(Path.GetFileNameWithoutExtension(module.Name), monoScripts);
            }
        }

        private bool InheritsFrom(TypeDef type, string baseTypeFullName)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.FullName == baseTypeFullName)
                    return true;

                try
                {
                    var resolved = current.ResolveTypeDef();
                    if (resolved == null)
                        break;
                    current = resolved.BaseType;
                }
                catch
                {
                    break;
                }
            }
            return false;
        }
    }
}
