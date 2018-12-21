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
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
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

        public async Task<List<CSharpIndexerResult>> BuildIndexerAsync(List<Document> documents)
        {
            var results = new List<CSharpIndexerResult>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var document in documents)
            {
                if (!Regex.IsMatch(document.FilePath, @"(\\service|\\TemporaryGeneratedFile_.*|\\assemblyinfo|\\assemblyattributes|\.(g\.i|g|designer|generated|assemblyattributes))\.(cs|vb)$"))
                {
                    results.Add(await BuildFromDocumentAsync(document));
                }          
            }
            sw.Stop();
            OutputWindowLogger.WriteLn($"Building Queries db from code ended in {sw.ElapsedMilliseconds} ms. Found {results.Count} queries. In {documents.Count} documents.");
            return results;
        }

        public async Task<CSharpIndexerResult> BuildFromDocumentAsync(Document document)
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

        private CSharpIndexerResult Build(SemanticModel semModel, Document document)
        {
            var queryResults = new List<CSharpQuery>();
            var genericResults = new List<ExpressionResult>();
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
                    var expressionResult = new ExpressionResolver().GetStringValueOfExpression(node.Arguments.FirstOrDefault().Expression, nodes, semModel);
                    if (expressionResult.IsSolved)
                    {
                        queryResults.Add(new CSharpQuery
                        {
                            QueryFileName = Path.GetFileName(document.FilePath),
                            QueryId = expressionResult.TextResult,
                            QueryLineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            QueryVsProjectName = document.Project.Name,
                            QueryFilePath = document.FilePath,
                            DocumentId = document.Id,
                        });
                    }
                    else
                    {  
                        genericResults.Add(expressionResult);
                    }
                }
            }

            return new CSharpIndexerResult
            {
                Queries = queryResults,
                Generics = genericResults,
            };
        }
    }
}
