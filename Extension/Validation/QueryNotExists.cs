using IBatisSuperHelper;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver;
using IBatisSuperHelper.Storage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Validation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QueryNotExists : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "IB001";

        public static readonly DiagnosticDescriptor QueryNotExistsRule = new DiagnosticDescriptor(
            DiagnosticId,
            "Query not exist in any map",
            "Query '{0}' do not exist in any map file",
            "iBatis",
            DiagnosticSeverity.Warning,
            true
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(QueryNotExistsRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            NodeHelpers helper = new NodeHelpers(context.SemanticModel);

            if (helper.IsIBatisMethod(context.Node as InvocationExpressionSyntax))
            {
                if (helper.TryGetArgumentNodeFromInvocation(context.Node as InvocationExpressionSyntax, 0, out ExpressionSyntax expressionSyntax))
                {
                    var resolverResult = new ExpressionResolver().GetStringValueOfExpression(expressionSyntax, context.SemanticModel);
                    if (resolverResult.IsSolved)
                    {
                        var queryKeys = GotoAsyncPackage.Storage.XmlQueries.GetKeysByQueryId(resolverResult.TextResult, GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces);
                        if (queryKeys.Count < 1)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(QueryNotExistsRule, expressionSyntax.GetLocation(), resolverResult.TextResult));
                        }
                    }
                }

            }
        }
    }
}
