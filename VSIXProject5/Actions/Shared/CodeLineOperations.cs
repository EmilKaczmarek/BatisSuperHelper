using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Helpers;

namespace VSIXProject5.Actions.Shared
{
    public class CodeLineOperations : ILineOperation
    {
        private ITextSnapshot _textSnapshot;
        private IEnumerable<SyntaxNode> _lineNodes;
        private NodeHelpers _helperInstance;
        private Lazy<IEnumerable<SyntaxNode>> _documentNodes;

        public CodeLineOperations(ITextSnapshot snapshot, int selectionLineNumber)
        {
            _textSnapshot = snapshot;
            Document doc = _textSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            SemanticModel semModel = doc.GetSemanticModelAsync().Result;
            doc.TryGetSyntaxTree(out SyntaxTree synTree);

            _helperInstance = new NodeHelpers(semModel);

            var span = synTree.GetText().Lines[selectionLineNumber].Span;
            var root = (CompilationUnitSyntax)synTree.GetRoot();
            _lineNodes = root.DescendantNodesAndSelf(span);
            _documentNodes = new Lazy<IEnumerable<SyntaxNode>>(() => root.DescendantNodesAndSelf());
        }

        public string GetQueryNameAtLine()
        { 
            var returnNode = _helperInstance.GetFirstNodeOfReturnStatmentSyntaxType(_lineNodes);
            //Check if current document line is having 'return' keyword.
            //In this case we need to Descendant Node to find ArgumentList
            if (returnNode != null)
            {
                var returnNodeDescendanted = returnNode.DescendantNodesAndSelf();
                return _helperInstance.GetQueryStringFromSyntaxNodes(_lineNodes, _documentNodes.Value);
            }
            //In case we don't have cursor around 'return', SyntaxNodes taken from line
            //should have needed ArgumentLineSyntax
            return _helperInstance.GetQueryStringFromSyntaxNodes(_lineNodes, _documentNodes.Value);
        }

        public bool CanRenameQueryAtLine()
        {
            return _helperInstance.GetExpressionKindForBatisMethodArgument(_lineNodes) == SyntaxKind.StringLiteralExpression;        
        }
    }
}
