using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using VSIXProject5.Validation;
using IBatisSuperHelper.Storage;
using Microsoft.CodeAnalysis.Editing;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Validation.Helpers;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;

namespace IBatisSuperHelper.Validation
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QueryNotExistsCodeFixProvider)), Shared]
    public class QueryNotExistsCodeFixProvider : CodeFixProvider
    {
        private string titleFormat = $"Change query name to {0}";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(QueryNotExists.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(diagnosticSpan);
            
            LiteralExpressionSyntax stringLiteral = node is LiteralExpressionSyntax
                ? node as LiteralExpressionSyntax
                : (node as ArgumentSyntax).Expression as LiteralExpressionSyntax;

            
            if (stringLiteral != null && new NodeHelpers().TryGetQueryStringFromStringLiteralNode(stringLiteral, out string queryName))
            {
                foreach (var proposition in new QuerySearchHelper().GetPropositionsByStatmentName(queryName))
                {
                    var newQuery = PackageStorage.XmlQueries.GetValueOrNull(proposition);
                    context.RegisterCodeFix(
                   CodeAction.Create(
                       title: $"Change to: {newQuery.QueryId} in: {newQuery.MapNamespace}",
                       createChangedDocument: c => SwitchQuery(context.Document, stringLiteral, newQuery, c),
                       equivalenceKey: $"Change to: {newQuery.QueryId} in: {newQuery.MapNamespace}"),
                   diagnostic);
                }
            }
        }

        private async Task<Document> SwitchQuery(Document document, LiteralExpressionSyntax stringLiteralExpression, XmlQuery newQuery, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            editor.ReplaceNode(stringLiteralExpression,
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(newQuery.QueryId)
                    )
                );

            return editor.GetChangedDocument();
        }

    }
}
