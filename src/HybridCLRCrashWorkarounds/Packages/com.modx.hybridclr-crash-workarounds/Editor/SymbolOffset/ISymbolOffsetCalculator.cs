namespace HybridCLR.Editor.CrashWorkarounds
{
    public interface ISymbolOffsetCalculator
    {
        int? GetOffset(Architecture architecture);
        void Run();
    }
}
