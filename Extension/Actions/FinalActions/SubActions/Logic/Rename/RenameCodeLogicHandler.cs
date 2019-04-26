using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Windows.RenameWindow.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Linq;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Logic.Rename
{
    public class RenameCodeLogicHandler
    {
        public bool ExecuteRename(KeyValuePair<IndexerKey, List<CSharpQuery>> codeKeyValuePair, RenameViewModel renameViewModel, VisualStudioWorkspace workspace)
        {
            bool result = true;
            foreach (var file in codeKeyValuePair.Value.GroupBy(x => x.DocumentId, x => x))
            {
                var doc = workspace.CurrentSolution.GetDocument(file.Key);
                SemanticModel semModel = ThreadHelper.JoinableTaskFactory.Run(async () => await doc.GetSemanticModelAsync());
                NodeHelpers helper = new NodeHelpers(semModel);
                doc.TryGetSyntaxTree(out SyntaxTree synTree);
                var root = (CompilationUnitSyntax)synTree.GetRoot();
                var newArgumentSyntax = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(renameViewModel.QueryText)));

                foreach (var query in file)
                {
                    var span = synTree.GetText().Lines[query.QueryLineNumber - 1].Span;
                    var nodes = root.DescendantNodesAndSelf(span);
                    var existingQueryArgumentSyntax = helper.GetProperArgumentNodeInNodes(nodes);

                    var newRoot = root.ReplaceNode(existingQueryArgumentSyntax, newArgumentSyntax);
                    root = newRoot;

                    doc = doc.WithText(newRoot.GetText());
                    var applyResult = workspace.TryApplyChanges(doc.Project.Solution);
                    result = result && applyResult;
                }
            }
            return result;
        }
    }
}
