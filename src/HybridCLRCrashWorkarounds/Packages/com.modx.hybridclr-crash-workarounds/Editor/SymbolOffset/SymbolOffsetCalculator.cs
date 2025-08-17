namespace HybridCLR.Editor.CrashWorkarounds
{
    public sealed class SymbolOffsetCalculatorFactory
    {
        private const string TargetSymbolPattern = ".*CreateMonoScriptFromScriptingType.*ScriptingClassPtr.*";
        private const string DefaultBaseSymbolPattern = ".*GameObject_CUSTOM_Internal_AddComponentWithType.*ScriptingBackendNativeObjectPtrOpaque.*";

        public static ISymbolOffsetCalculator CreateForAndroid(string gradleProjectDir, bool developmentBuild)
        {
            return new SymbolOffsetCalculatorAndroid(TargetSymbolPattern, DefaultBaseSymbolPattern, gradleProjectDir, developmentBuild);
        }
    }
}
