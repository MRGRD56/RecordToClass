using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RecordToClass.Extensions;

namespace RecordToClass.Models
{
    public record Record(
        SyntaxTree SyntaxTree)
    {
        public static Record Parse(string recordString)
        {
            return new Record(CSharpSyntaxTree.ParseText(recordString));
        }

        public string ToRecordString()
        {
            return SyntaxTree.GetRoot().Format().ToString();
        }

        public string ToClassString(
            PropertyAccessor propertiesAccessor = PropertyAccessor.Get,
            bool useThisKeyword = false,
            bool createDeconstructor = true)
        {
            var thisKeyword = useThisKeyword ? "this." : "";
            var propertiesAccessors = PropertyAccessorsDictionary[propertiesAccessor];

            var codeRoot = (CompilationUnitSyntax)SyntaxTree.GetRoot();
            var recordElement = (RecordDeclarationSyntax)codeRoot.Members[0];
            var modifiers = recordElement.Modifiers;
            var baseList = recordElement.BaseList;
            var name = recordElement.Identifier;
            var parameters = recordElement.ParameterList;
            var members = recordElement.Members;
            var attributes = recordElement.AttributeLists;

            var properties = parameters?.Parameters.Select(p =>
            {
                return SyntaxFactory.PropertyDeclaration(
                        p.Type,
                        p.Identifier)
                    .WithAttributeLists(p.AttributeLists)
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(propertiesAccessors);
            }).ToArray();

            var classRootElement = SyntaxFactory.CompilationUnit();

            var classElement = SyntaxFactory.ClassDeclaration(name)
                .WithAttributeLists(attributes)
                .WithBaseList(baseList)
                .WithModifiers(modifiers);

            if (properties?.Any() == true)
            {
                classElement = classElement.AddMembers(properties);
            }

            var classElementProperties = classElement.Members
                .OfType<PropertyDeclarationSyntax>()
                .ToArray();


            if (properties?.Any() == true)
            {
                var constructorParameters = classElementProperties
                    .Select(p =>
                    {
                        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(ToCamelCase(p.Identifier.Text)))
                            .WithType(p.Type);
                    })
                    .ToArray();

                var constructorStatements = classElementProperties
                    .Select(p =>
                    {
                        var operandsName = (Left: p.Identifier.Text, Right: ToCamelCase(p.Identifier.Text));
                        var @this = operandsName.Left == operandsName.Right ? "this." : thisKeyword;
                        return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(@this + operandsName.Left),
                                SyntaxFactory.IdentifierName(operandsName.Right)));
                    })
                    .ToArray();

                classElement = classElement
                    .AddMembers(
                        SyntaxFactory.ConstructorDeclaration(name)
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(constructorParameters)
                            .WithBody(SyntaxFactory.Block(constructorStatements)));
            }

            classElement = classElement.AddMembers(members.ToArray());

            classRootElement = classRootElement.AddMembers(classElement);

            var workspace = new AdhocWorkspace();
            workspace.AddSolution(
                SolutionInfo.Create(SolutionId.CreateNewId("formatter"),
                    VersionStamp.Default)
            );

            return classRootElement.Format().ToString();
        }

        public override string ToString() => ToRecordString();

        private static AccessorDeclarationSyntax GetPropertyAccessor(SyntaxKind syntaxKind)
        {
            return SyntaxFactory.AccessorDeclaration(syntaxKind)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
        
        private static readonly Dictionary<PropertyAccessor, AccessorDeclarationSyntax[]> PropertyAccessorsDictionary = new()
        {
            {
                PropertyAccessor.GetSet,
                new[]
                {
                    GetPropertyAccessor(SyntaxKind.GetAccessorDeclaration),
                    GetPropertyAccessor(SyntaxKind.SetAccessorDeclaration)
                }
            },
            {
                PropertyAccessor.Get,
                new[]
                {
                    GetPropertyAccessor(SyntaxKind.GetAccessorDeclaration)
                }
            },
            {
                PropertyAccessor.Set,
                new[]
                {
                    GetPropertyAccessor(SyntaxKind.SetAccessorDeclaration)
                }
            },
            {
                PropertyAccessor.GetInit,
                new[]
                {
                    GetPropertyAccessor(SyntaxKind.GetAccessorDeclaration),
                    GetPropertyAccessor(SyntaxKind.InitAccessorDeclaration)
                }
            }
        };

        private static string ToCamelCase(string pascalCase)
        {
            var stringBuilder = new StringBuilder(pascalCase);
            stringBuilder[0] = char.ToLower(stringBuilder[0]);
            return stringBuilder.ToString();
        }
    }
}