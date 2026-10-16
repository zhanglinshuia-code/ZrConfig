using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ZrConfig.Core;
/// <summary>Keep emitted C# deterministic and compatible with Unity's C# 9 compiler.</summary>
internal static class GeneratedCodeStyle
{
    public static string Format(string source, params string[] symbols)
    {
        CompilationUnitSyntax root = SyntaxFactory.ParseCompilationUnit(source,
            options: new CSharpParseOptions(LanguageVersion.Preview,
            preprocessorSymbols: symbols));
        root = (CompilationUnitSyntax)new BlockRewriter().Visit(root)!;
        root = root.WithUsings(SyntaxFactory.List(root.Usings.OrderBy(item => item.Name?.ToString()
            .StartsWith("System",
            StringComparison.Ordinal) == true ? 0 : 1)
            .ThenBy(item => item.Name?.ToString(),
            StringComparer.Ordinal)));
        root = root.NormalizeWhitespace(indentation: "    ", eol: "\n");
        root = (CompilationUnitSyntax)new LayoutRewriter().Visit(root)!;
        string formatted = string.Join('\n', root.ToFullString().Split('\n').Select(line => line.TrimEnd()));
        return WrapLongLines(formatted.TrimEnd() + "\n", symbols);
    }

    internal static string WrapLongLines(string source, params string[] symbols)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: symbols));
        SourceText text = tree.GetText();
        var changes = new SortedDictionary<int, TextChange>();
        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            if (node.Ancestors().Any(ancestor => ancestor is InterpolationSyntax))
            {
                continue;
            }

            if (node is MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax, Parent: InvocationExpressionSyntax } member)
            {
                BreakBefore(member.OperatorToken);
            }
            else if (node is BaseArgumentListSyntax arguments)
            {
                foreach (SyntaxToken comma in arguments.Arguments.GetSeparators())
                {
                    BreakBefore(comma.GetNextToken());
                }
            }
            else if (node is BaseParameterListSyntax parameters)
            {
                foreach (SyntaxToken comma in parameters.Parameters.GetSeparators())
                {
                    BreakBefore(comma.GetNextToken());
                }
            }
        }

        return text.WithChanges(changes.Values).ToString();

        void BreakBefore(SyntaxToken token)
        {
            TextLine line = text.Lines.GetLineFromPosition(token.SpanStart);
            string content = line.ToString();
            if (content.Length <= 140)
            {
                return;
            }

            int start = token.SpanStart;
            while (start > line.Start && source[start - 1] == ' ')
            {
                start--;
            }

            if (start == line.Start)
            {
                return;
            }

            int indent = content.TakeWhile(character => character == ' ').Count() + 4;
            changes[start] = new TextChange(TextSpan.FromBounds(start, token.SpanStart), "\n" + new string(' ', indent));
        }
    }

    // NormalizeWhitespace does not honor EditorConfig: it keeps auto-accessors on one
    // line and inserts spaces in nullable casts and omitted array dimensions.
    private sealed class LayoutRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitAccessorList(AccessorListSyntax node)
        {
            node = (AccessorListSyntax)base.VisitAccessorList(node)!;
            if (node.Accessors.Any(accessor => accessor.Body != null || accessor.ExpressionBody != null))
            {
                return node;
            }

            string indent = new(' ', node.Ancestors().Count(ancestor => ancestor is BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax) * 4);
            return node.WithOpenBraceToken(node.OpenBraceToken
                    .WithLeadingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.Whitespace(indent))
                    .WithTrailingTrivia(SyntaxFactory.LineFeed))
                .WithAccessors(SyntaxFactory.List(node.Accessors.Select(accessor => accessor
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(indent + "    "))
                    .WithTrailingTrivia(SyntaxFactory.LineFeed))))
                .WithCloseBraceToken(node.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.Whitespace(indent)));
        }

        public override SyntaxNode? VisitNullableType(NullableTypeSyntax node)
        {
            node = (NullableTypeSyntax)base.VisitNullableType(node)!;
            return node.GetLastToken().GetNextToken().Kind() is SyntaxKind.CloseParenToken or SyntaxKind.OpenBracketToken
                ? node.WithQuestionToken(node.QuestionToken.WithTrailingTrivia())
                : node;
        }

        public override SyntaxNode? VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
        {
            node = (ArrayRankSpecifierSyntax)base.VisitArrayRankSpecifier(node)!;
            return node.Sizes.All(size => size is OmittedArraySizeExpressionSyntax)
                ? node.WithSizes(SyntaxFactory.SeparatedList(node.Sizes.Select(size => size.WithoutTrivia()),
                    node.Sizes.GetSeparators().Select(comma => comma.WithoutTrivia())))
                : node;
        }
    }

    private sealed class BlockRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitBlock(BlockSyntax node)
        {
            node = (BlockSyntax)base.VisitBlock(node)!;
            return node.WithStatements(SyntaxFactory.List(node.Statements.SelectMany(SplitDeclaration)));
        }

        public override SyntaxNode? VisitSwitchSection(SwitchSectionSyntax node)
        {
            node = (SwitchSectionSyntax)base.VisitSwitchSection(node)!;
            return node.WithStatements(SyntaxFactory.List(node.Statements.SelectMany(SplitDeclaration)));
        }

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node)!;
            return node.WithMembers(SyntaxFactory.List(node.Members.SelectMany(SplitField)));
        }

        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
        {
            node = (StructDeclarationSyntax)base.VisitStructDeclaration(node)!;
            return node.WithMembers(SyntaxFactory.List(node.Members.SelectMany(SplitField)));
        }

        private static IEnumerable<StatementSyntax> SplitDeclaration(StatementSyntax statement)
        {
            if (statement is not LocalDeclarationStatementSyntax local || local.Declaration.Variables.Count < 2)
            {
                yield return statement;
                yield break;
            }

            for (int index = 0; index < local.Declaration.Variables.Count; index++)
            {
                LocalDeclarationStatementSyntax item = local.WithDeclaration(local.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(local.Declaration.Variables[index])));
                if (index > 0)
                {
                    item = item.WithoutLeadingTrivia();
                }

                if (index < local.Declaration.Variables.Count - 1)
                {
                    item = item.WithoutTrailingTrivia();
                }

                yield return item;
            }
        }

        private static IEnumerable<MemberDeclarationSyntax> SplitField(MemberDeclarationSyntax member)
        {
            if (member is not FieldDeclarationSyntax field || field.Declaration.Variables.Count < 2)
            {
                yield return member;
                yield break;
            }

            for (int index = 0; index < field.Declaration.Variables.Count; index++)
            {
                FieldDeclarationSyntax item = field.WithDeclaration(field.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(field.Declaration.Variables[index])));
                if (index > 0)
                {
                    item = item.WithoutLeadingTrivia();
                }

                if (index < field.Declaration.Variables.Count - 1)
                {
                    item = item.WithoutTrailingTrivia();
                }

                yield return item;
            }
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            node = (IfStatementSyntax)base.VisitIfStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitElseClause(ElseClauseSyntax node)
        {
            node = (ElseClauseSyntax)base.VisitElseClause(node)!;
            return node.Statement is IfStatementSyntax ? node : node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitForStatement(ForStatementSyntax node)
        {
            node = (ForStatementSyntax)base.VisitForStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node)
        {
            node = (ForEachStatementSyntax)base.VisitForEachStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            node = (ForEachVariableStatementSyntax)base.VisitForEachVariableStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            node = (WhileStatementSyntax)base.VisitWhileStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            node = (DoStatementSyntax)base.VisitDoStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitUsingStatement(UsingStatementSyntax node)
        {
            node = (UsingStatementSyntax)base.VisitUsingStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitLockStatement(LockStatementSyntax node)
        {
            node = (LockStatementSyntax)base.VisitLockStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        public override SyntaxNode? VisitFixedStatement(FixedStatementSyntax node)
        {
            node = (FixedStatementSyntax)base.VisitFixedStatement(node)!;
            return node.WithStatement(Block(node.Statement));
        }

        private static StatementSyntax Block(StatementSyntax statement) => statement is BlockSyntax ? statement : SyntaxFactory.Block(statement);
    }
}
