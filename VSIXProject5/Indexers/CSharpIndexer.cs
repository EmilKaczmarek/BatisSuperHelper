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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions.Roslyn;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Loggers;
using VSIXProject5.Models;

namespace VSIXProject5.Indexers
{
    public class CSharpIndexer
    {
        private readonly Workspace _workspace;

        public CSharpIndexer()
        {

        }

        public CSharpIndexer(Workspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<List<List<CSharpIndexerResult>>> BuildIndexerAsync(List<Document> documents)
        {
            var result = new List<List<CSharpIndexerResult>>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var document in documents)
            {
                if (!Regex.IsMatch(document.FilePath, @"(\\service|\\TemporaryGeneratedFile_.*|\\assemblyinfo|\\assemblyattributes|\.(g\.i|g|designer|generated|assemblyattributes))\.(cs|vb)$"))
                {
                    result.Add(await BuildFromDocumentAsync(document));
                }          
            }
            sw.Stop();
            OutputWindowLogger.WriteLn($"Building Queries db from code ended in {sw.ElapsedMilliseconds} ms. Found {result.Count} queries. In {documents.Count} documents.");
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
             
                var allowedTypes = new List<Type> { typeof(IdentifierNameSyntax), typeof(GenericNameSyntax) };
                var nameIdentifiers = firstInvocationOfNodeAncestors.Expression.DescendantNodes().Where(e=>allowedTypes.Contains(e.GetType())).Cast<SimpleNameSyntax>();

                if (nameIdentifiers.Any(e => IBatisConstants.MethodNames.Contains(e.Identifier.ValueText)))
                {
                    Location loc = Location.Create(synTree, node.Span);
                    var queryName = new ExpressionResolver().GetStringValueOfExpression(node.Arguments.FirstOrDefault().Expression, nodes, semModel);
                    if (queryName != "")
                    {
                        result.Add(new CSharpIndexerResult
                        {
                            QueryFileName = Path.GetFileName(document.FilePath),
                            QueryId = queryName,
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
