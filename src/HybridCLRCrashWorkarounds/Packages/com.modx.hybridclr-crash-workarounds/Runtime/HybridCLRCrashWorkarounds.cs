using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace HybridCLR
{
    public static class HybridCLRCrashWorkarounds
    {
#if (UNITY_IOS || UNITY_ANDROID || UNITY_OPENHARMONY) && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern unsafe int CreateMonoScriptsInternal(byte* data, int length, [MarshalAs(UnmanagedType.U1)] bool isDebugBuild, out int errorMessageLength, out int exceptionMessageLength);

        [DllImport("__Internal")]
        private static extern unsafe void GetErrorMessageAndClear(byte* buffer);

        [DllImport("__Internal")]
        private static extern unsafe void GetExceptionMessageAndClear(byte* buffer);
#endif

#if (UNITY_ANDROID || UNITY_OPENHARMONY) && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern unsafe void SetCreateMonoScriptFromScriptingTypeSymbolOffset(int symbolOffset);

        [DllImport("__Internal")]
        private static extern unsafe int GetCreateMonoScriptFromScriptingTypeSymbolOffset();
#endif

        public static int SymbolOffset
        {
            get
            {
#if (UNITY_ANDROID || UNITY_OPENHARMONY) && !UNITY_EDITOR
                return GetCreateMonoScriptFromScriptingTypeSymbolOffset();
#else
                return 0;
#endif
            }

            set
            {
#if (UNITY_ANDROID || UNITY_OPENHARMONY) && !UNITY_EDITOR
                SetCreateMonoScriptFromScriptingTypeSymbolOffset(value);
#endif
            }
        }

        public static void CreateMonoScripts(byte[] data)
        {
#if (UNITY_ANDROID || UNITY_OPENHARMONY) && !UNITY_EDITOR
            if (SymbolOffset == 0)
                throw new Exception("Invalid symbol offset");
#endif

#if (UNITY_IOS || UNITY_ANDROID || UNITY_OPENHARMONY) && !UNITY_EDITOR
            unsafe
            {
                int errorMessageLength;
                int exceptionMessageLength;

                fixed (byte* ptr = data)
                {
                    int count = CreateMonoScriptsInternal(ptr, data.Length, Debug.isDebugBuild, out errorMessageLength, out exceptionMessageLength);

                    Debug.Log($"Created {count} MonoScripts");
                }

                if (errorMessageLength > 0)
                {
                    var buffer = new byte[errorMessageLength];

                    fixed (byte* ptr = buffer)
                    {
                        GetErrorMessageAndClear(ptr);
                    }

                    Debug.LogError(Encoding.UTF8.GetString(buffer));
                }

                if (exceptionMessageLength > 0)
                {
                    var buffer = new byte[exceptionMessageLength];

                    fixed (byte* ptr = buffer)
                    {
                        GetExceptionMessageAndClear(ptr);
                    }

                    throw new Exception(Encoding.UTF8.GetString(buffer));
                }
            }
#endif
        }
    }
}
