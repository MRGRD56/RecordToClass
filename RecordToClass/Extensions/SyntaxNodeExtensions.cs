using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace RecordToClass.Extensions
{
    public static class SyntaxNodeExtensions
    {
        private static readonly AdhocWorkspace FormatterWorkspace;

        static SyntaxNodeExtensions()
        {
            FormatterWorkspace = new AdhocWorkspace();
            FormatterWorkspace.AddSolution(
                SolutionInfo.Create(SolutionId.CreateNewId("formatter"),
                    VersionStamp.Default));
        }

        public static SyntaxNode Format(this SyntaxNode node) => Formatter.Format(node, FormatterWorkspace);
    }
}