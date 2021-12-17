namespace TestsGeneratorLib
{
    public class TestUnit
    {
        public string FileName { get; }
        public string SourceCode { get; }

        internal TestUnit(string fileName, string sourceCode)
        {
            FileName = fileName;
            SourceCode = sourceCode;
        }
    }
}