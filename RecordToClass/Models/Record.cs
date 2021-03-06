using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RecordToClass.Extensions;

namespace RecordToClass.Models
{
    public class Record
    {
        private SyntaxTree SyntaxTree { get; }

        public Record(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }
        
        public RecordDeclarationSyntax Declaration
        {
            get
            {
                var codeRoot = (CompilationUnitSyntax)SyntaxTree.GetRoot();
                var recordDeclaration = (RecordDeclarationSyntax)codeRoot.Members[0];
                return recordDeclaration;
            }
        }
        
        public static Record Parse(string recordString)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(recordString);
            return new Record(syntaxTree);
        }

        public ClassDeclarationSyntax ToClass(
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

            var constructorParameters = parameters?.Parameters
                .Select(p =>
                {
                    return SyntaxFactory.Parameter(SyntaxFactory.Identifier(ToCamelCase(p.Identifier.Text)))
                        .WithType(p.Type)
                        .WithDefault(p.Default);
                })
                .ToArray();

            if (constructorParameters?.Any() == true)
            {
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

            if (createDeconstructor && constructorParameters?.Length >= 2)
            {
                var deconstructorParameters = constructorParameters
                    .Select(p => p
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword))
                        .WithDefault(null))
                    .ToArray();
                
                var deconstructorStatements = classElementProperties
                    .Select(p =>
                    {
                        var operandsName = (Left: ToCamelCase(p.Identifier.Text), Right: p.Identifier.Text);
                        var @this = operandsName.Left == operandsName.Right ? "this." : thisKeyword;
                        return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(operandsName.Left),
                                SyntaxFactory.IdentifierName(@this + operandsName.Right)));
                    })
                    .ToArray();

                classElement = classElement.AddMembers(
                    SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                            "Deconstruct")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(deconstructorParameters)
                        .WithBody(SyntaxFactory.Block(deconstructorStatements)));
            }

            classElement = classElement.AddMembers(members.ToArray());

            classRootElement = classRootElement.AddMembers(classElement);

            return classElement.Format();
        }

        public override string ToString() => Declaration.Format().ToString();

        private static AccessorDeclarationSyntax GetPropertyAccessor(SyntaxKind syntaxKind)
        {
            return SyntaxFactory.AccessorDeclaration(syntaxKind)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private static readonly Dictionary<PropertyAccessor, AccessorDeclarationSyntax[]> PropertyAccessorsDictionary =
            new()
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
            if (pascalCase.Length == 0) return pascalCase;
            var stringBuilder = new StringBuilder(pascalCase);
            stringBuilder[0] = char.ToLower(stringBuilder[0]);
            return stringBuilder.ToString();
        }
    }
}