using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Stopwatch sw = new Stopwatch();
            var helper = new NodeHelpers(semModel);
            var result = new List<CSharpIndexerResult>();
            SyntaxTree synTree = null;
            document.TryGetSyntaxTree(out synTree);
            var treeRoot = (CompilationUnitSyntax)synTree.GetRoot();

            //if (!HasBatisUsing(treeRoot))
            //{
            //    return new List<CSharpIndexerResult>();
            //}
          
            var nodes = treeRoot.DescendantNodesAndSelf();
            var argumentNodes = nodes
                .OfType<ArgumentListSyntax>()
                .Where(x => x.Arguments.Any())
                .Select(x => x)
                .ToList();
            foreach (var argumentNode in argumentNodes)
            {
                if (argumentNode is ArgumentListSyntax)
                {
                    var nodeAncestors = argumentNode.Ancestors().OfType<InvocationExpressionSyntax>().ToList();
                    if (nodeAncestors.Any(x =>
                         semModel.GetSymbolInfo(x).Symbol?.ContainingNamespace?.ToDisplayString().Contains("Batis") ==  true
                    ))
                    {
                        Location loc = Location.Create(synTree, argumentNode.Span);
                        IndexerKey key = IndexerKey.ConvertToKey(argumentNode.Arguments.FirstOrDefault().ToCleanString(), document.Project.Name);
                        result.Add(new CSharpIndexerResult
                        {
                            QueryFileName = Path.GetFileName(document.FilePath),
                            QueryId = argumentNode.Arguments.FirstOrDefault().ToCleanString(),
                            QueryLineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            QueryVsProjectName = document.Project.Name,
                            QueryFilePath = document.FilePath,
                            DocumentId = document.Id,
                        });
                    }
                }
            }
            sw.Stop();
            Debug.WriteLine($"Build: {sw.Elapsed}");
            return result;
        }
    }
}
