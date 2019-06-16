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
using BatisSuperHelper.Storage;
using Microsoft.CodeAnalysis.Editing;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.Validation.Helpers;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;

namespace BatisSuperHelper.Validation
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QueryNotExistsCodeFixProvider)), Shared]
    public class QueryNotExistsCodeFixProvider : CodeFixProvider
    {
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
                    var newQuery = GotoAsyncPackage.Storage.XmlQueries.GetValueOrNull(proposition);
                    context.RegisterCodeFix(
                   CodeAction.Create(
                       title: $"Change to: {newQuery.QueryId} in: {newQuery.MapNamespace}",
                       createChangedDocument: c => SwitchQueryAsync(context.Document, stringLiteral, newQuery, c),
                       equivalenceKey: $"Change to: {newQuery.QueryId} in: {newQuery.MapNamespace}"),
                   diagnostic);
                }
            }
        }

        private async Task<Document> SwitchQueryAsync(Document document, LiteralExpressionSyntax stringLiteralExpression, Statement newQuery, CancellationToken cancellationToken)
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
