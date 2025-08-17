using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HybridCLR.Editor.CrashWorkarounds
{
    internal sealed class MonoScriptReflector : IMonoScriptProvider
    {
        public Dictionary<string, Dictionary<string, List<string>>> MonoScripts => mMonoScripts;

        private readonly HashSet<string> mDlls;
        private readonly Dictionary<string, Dictionary<string, List<string>>> mMonoScripts = new Dictionary<string, Dictionary<string, List<string>>>();

        public MonoScriptReflector(IEnumerable<string> dlls)
        {
            mDlls = new HashSet<string>(dlls);
        }

        public void Run()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => mDlls.Contains(x.GetName().Name));

            foreach (var assembly in assemblies)
            {
                var monoScripts = new Dictionary<string, List<string>>();

                foreach (var type in assembly.GetTypes())
                {
                    if (type.BaseType == null)
                        continue;

                    if (type.IsAbstract)
                        continue;

                    if (type.IsGenericType)
                        continue;

                    if (type.DeclaringType != null)
                        continue;

                    if (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        var ns = type.Namespace != null ? type.Namespace.ToString() : string.Empty;
                        if (!monoScripts.TryGetValue(ns, out var list))
                        {
                            list = new List<string>();
                            monoScripts.Add(ns, list);
                        }

                        list.Add(type.Name);
                    }
                }

                if (monoScripts.Count > 0)
                    mMonoScripts.Add(assembly.GetName().Name, monoScripts);
            }
        }
    }
}
