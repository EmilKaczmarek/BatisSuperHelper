using Microsoft.CodeAnalysis;
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
        public string GetQueryNameAtLine(ITextSnapshot snapshot, int selectionLineNumber)
        {
            Document doc = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            SemanticModel semModel = doc.GetSemanticModelAsync().Result;
            doc.TryGetSyntaxTree(out SyntaxTree synTree);

            NodeHelpers helper = new NodeHelpers(semModel);
            
            var span = synTree.GetText().Lines[selectionLineNumber].Span;
            var root = (CompilationUnitSyntax)synTree.GetRoot();
            var nodesAtLine = root.DescendantNodesAndSelf(span);
            var returnNode = helper.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
            //Check if current document line is having 'return' keyword.
            //In this case we need to Descendant Node to find ArgumentList
            if (returnNode != null)
            {
                var returnNodeDescendanted = returnNode.DescendantNodesAndSelf();
                return helper.GetQueryStringFromSyntaxNodes(returnNodeDescendanted, root.DescendantNodesAndSelf());
            }
            //In case we don't have cursor around 'return', SyntaxNodes taken from line
            //should have needed ArgumentLineSyntax
            return helper.GetQueryStringFromSyntaxNodes(nodesAtLine, root.DescendantNodesAndSelf());
        }
    }
}
