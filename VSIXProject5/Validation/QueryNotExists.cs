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
using VSIXProject5.Helpers;
using VSIXProject5.Indexers;

namespace VSIXProject5.Validation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QueryNotExists : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor QueryNotExistsRule = new DiagnosticDescriptor(
            "QueryNotExistId",
            "Query not exist in any map",
            "Query '{0}' do not exist in any map file",
            "iBatis",
            DiagnosticSeverity.Warning,
            true
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(QueryNotExistsRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.StringLiteralExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            NodeHelpers helper = new NodeHelpers(context.SemanticModel);
            var nodes = context.Node.Parent;

            var test = context.SemanticModel.SyntaxTree.GetLineSpan(context.Node.Span);
            var lineSpan = context.SemanticModel.SyntaxTree.GetText().Lines[test.StartLinePosition.Line].Span;
            var treeRoot = (CompilationUnitSyntax)context.SemanticModel.SyntaxTree.GetRoot();
            var nodesAtLine = treeRoot.DescendantNodesAndSelf(lineSpan);

            var returnNode = helper.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
            if (returnNode != null)
            {
                nodesAtLine = returnNode.DescendantNodesAndSelf();
            }
            var validLine = helper.IsAnySyntaxNodeContainIBatisNamespace(nodesAtLine);
            if (validLine)
            {
                var queryName = helper.GetQueryStringFromSyntaxNodes(nodesAtLine);
                var queryKeys = Indexer.Instance.GetXmlKeysByQueryId(queryName);
                if (queryKeys.Count == 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(QueryNotExistsRule, context.Node.GetLocation(), queryName));
                }               
            } 
        }
    }
}
