using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public static TSyntaxNode Format<TSyntaxNode>(this TSyntaxNode node) where TSyntaxNode : SyntaxNode =>
            (TSyntaxNode)Formatter.Format(node, FormatterWorkspace);

        public static async Task<TSyntaxNode> FormatAsync<TSyntaxNode>(this TSyntaxNode node) where TSyntaxNode : SyntaxNode
        {
            TSyntaxNode formattedNode = null;
            await Task.Run(() =>
            {
                formattedNode = node.Format();
            });
            return formattedNode;
        }

        public static string FormatCode(string unformattedCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(unformattedCode);
            return syntaxTree.GetRoot().Format().ToString();
        }
        
        public static async Task<string> FormatCodeAsync(string unformattedCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(unformattedCode);
            return (await (await syntaxTree.GetRootAsync()).FormatAsync()).ToString();
        }
    }
}