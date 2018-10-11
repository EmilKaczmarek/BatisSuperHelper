using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions.Roslyn;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Models;

namespace VSIXProject5.Indexers
{
    public class CSharpIndexer: BaseIndexer
    {
        private readonly Workspace _workspace;
        public CSharpIndexer()
        {
            //var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            //_workspace = componentModel.GetService<VisualStudioWorkspace>();
        }
        public CSharpIndexer(Workspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<List<CSharpIndexerResult>> BuildIndexerAsync(List<Document> documents)
        {
            var result = new List<CSharpIndexerResult>();

            foreach (var document in documents)
            { 
                result.AddRange(await BuildFromDocumentAsync(document));
            }

            return result;
        }

        public async Task<List<CSharpIndexerResult>> BuildFromDocumentAsync(Document document)
        {
            SemanticModel semModel = await document.GetSemanticModelAsync();
            return Build(semModel, document);
        }

        public bool HasBatisUsing(CompilationUnitSyntax treeRoot)
        {
            var identifiers = treeRoot.Usings.Select(x => x.Name).OfType<IdentifierNameSyntax>().ToList();
            if (identifiers.Any(e => e.Identifier.ValueText.ToLower().Contains("batis")))
            {
                return true;
            }

            var identifiers2 = treeRoot.Usings.Select(x => x.ToString()).ToList();
            if (identifiers2.Any(e => e.ToLower().Contains("batis")))
            {
                return true;
            }

            return false;
        }

        private List<CSharpIndexerResult> Build(SemanticModel semModel, Document document)
        {
            var result = new List<CSharpIndexerResult>();
            SyntaxTree synTree = null;
            document.TryGetSyntaxTree(out synTree);
            var treeRoot = (CompilationUnitSyntax)synTree.GetRoot();

            var nodes = treeRoot.DescendantNodesAndSelf();
            var argumentNodes = nodes
                .OfType<ArgumentListSyntax>()
                .Where(x => x.Arguments.Any());

            foreach (var node in argumentNodes)
            {
                var firstInvocationOfNodeAncestors = node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();

                if (firstInvocationOfNodeAncestors == null)
                    continue;
                
                var nameIdentifiers = firstInvocationOfNodeAncestors.Expression.DescendantNodes().OfType<IdentifierNameSyntax>();

                Debug.WriteLine(string.Join("\n", nameIdentifiers.Select(e =>e.Identifier.ValueText)));

                if (nameIdentifiers.Any(e => IBatisConstants.MethodNames.Contains(e.Identifier.ValueText)))
                {
                    Location loc = Location.Create(synTree, node.Span);
                    var constantValueFromExpression = semModel.GetConstantValue(node.Arguments.FirstOrDefault().Expression);
                    if(constantValueFromExpression.HasValue)
                    {
                        result.Add(new CSharpIndexerResult
                        {
                            QueryFileName = Path.GetFileName(document.FilePath),
                            QueryId = constantValueFromExpression.Value.ToString(),
                            QueryLineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            QueryVsProjectName = document.Project.Name,
                            QueryFilePath = document.FilePath,
                            DocumentId = document.Id,
                        });
                    }
                }
            }

            return result;
        }
    }
}
