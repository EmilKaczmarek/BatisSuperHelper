using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Models;

namespace VSIXProject5.Indexers
{
    public class CSharpIndexer
    {
        private Workspace _workspace;
        public CSharpIndexer()
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            _workspace = componentModel.GetService<VisualStudioWorkspace>();
        }
        /// <summary>
        /// FUCK TUPLES DELETE IT
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="solutionDir"></param>
        /// <returns></returns>
        public async Task<List<CSharpIndexerResult>> BuildFromFileAsync(Tuple<string, string> fileInfo, string solutionDir)
        {
            var documentid = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(fileInfo.Item1).FirstOrDefault();

            var docc = _workspace.CurrentSolution.GetDocument(documentid);

            return await BuildFromDocumentAsync(docc, fileInfo, solutionDir);
        }

        public async Task<List<CSharpIndexerResult>> BuildFromFileAsync(SimpleProjectItem fileInfo, string solutionDir)
        {
            var documentid = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(fileInfo.FilePath).FirstOrDefault();

            var docc = _workspace.CurrentSolution.GetDocument(documentid);

            return await BuildFromDocumentAsync(docc, fileInfo, solutionDir);
        }

        public async Task<List<CSharpIndexerResult>> BuildIndexerAsync(List<SimpleProjectItem> simpleProjectItems, string solutionDir)
        {
            var result = new List<CSharpIndexerResult>();

            foreach (var simpleProjectItem in simpleProjectItems)
            {
                result.AddRange(await BuildFromFileAsync(simpleProjectItem, solutionDir));
            }

            return result;
        }

        /// <summary>
        /// NOT USED FUCK TUPLES
        /// </summary>
        /// <param name="documentsFullNames"></param>
        /// <param name="solutionDir"></param>
        /// <returns></returns>
        public async Task<List<CSharpIndexerResult>> BuildIndexerAsync(List<Tuple<string,string>> documentsFullNames, string solutionDir)
        {
            var result = new List<CSharpIndexerResult>();

            foreach (var documentFullName in documentsFullNames)
            {
                result.AddRange(await BuildFromFileAsync(documentFullName, solutionDir));
            }

            return result;
        }

        public async Task<List<CSharpIndexerResult>> BuildFromDocumentAsync(Document document, Tuple<string, string> fileInfo, string solutionDir)
        {
            SemanticModel semModel2 = await document.GetSemanticModelAsync();
            return Build(semModel2, document, fileInfo, solutionDir);           
        }

        public async Task<List<CSharpIndexerResult>> BuildFromDocumentAsync(Document document, SimpleProjectItem fileInfo, string solutionDir)
        {
            SemanticModel semModel2 = await document.GetSemanticModelAsync();
            return Build(semModel2, document, fileInfo, solutionDir);
        }
        //public List<CSharpIndexerResult> BuildFromDocument(Document document, string fileDirectory, string solutionDir)
        //{
        //    SemanticModel semModel2 = document.GetSemanticModelAsync().Result;
        //    return Build(semModel2, document, fileDirectory, solutionDir);
        //}

        private List<CSharpIndexerResult> Build(SemanticModel semModel2, Document document, Tuple<string, string> fileInfo, string solutionName)
        {
            var result = new List<CSharpIndexerResult>();
            SyntaxTree synTree2 = null;
            document.TryGetSyntaxTree(out synTree2);
            var root2 = (CompilationUnitSyntax)synTree2.GetRoot();
            var nodes = root2.DescendantNodesAndSelf();
            var argumentNodes = nodes
                .OfType<ArgumentListSyntax>()                
                .Where(x => x.Arguments.Any())
                .Select(x => x)
                .ToList();
            foreach (var n in argumentNodes)
            {
                if (n is ArgumentListSyntax)
                {
                    var t = n.Ancestors().ToList();
                    if (t.Any(x =>
                         semModel2.GetSymbolInfo(x).Symbol != null &&
                         semModel2.GetSymbolInfo(x).Symbol.ContainingNamespace.ToDisplayString().Contains("Batis")
                    ))
                    {
                        Location loc = Location.Create(synTree2, n.Span);

                        string projectName = fileInfo.Item2;
                        var splitted = fileInfo.Item1.Split('\\').ToList();
                        var projectNameIndex = splitted.LastIndexOf(projectName);
                        var projectFilePath = splitted.Skip(projectNameIndex + 1);
                        string relativePath = $"{solutionName}\\{projectName}\\{string.Join("\\", projectFilePath)}";
                        CodeStatmentInfo csInfo = new CodeStatmentInfo
                        {
                            LineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            RelativePath = $"{relativePath.Replace(@"\\", @"\")}",
                            StatmentFile = Path.GetFileName(fileInfo.Item1),
                        };
                        IndexerKey key = IndexerKey.ConvertToKey(n.Arguments.FirstOrDefault().ToString().Replace("\"", "").Trim(), document.Project.Name);
                        result.Add(new CSharpIndexerResult
                        {
                            QueryFileName = Path.GetFileName(fileInfo.Item1),
                            QueryId = n.Arguments.FirstOrDefault().ToString().Replace("\"", "").Trim(),
                            QueryLineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            QueryVsProjectName = document.Project.Name,
                            QueryFilePath = fileInfo.Item1,
                        });
                    }
                }
            }
            return result;
        }

        private List<CSharpIndexerResult> Build(SemanticModel semModel2, Document document, SimpleProjectItem fileInfo, string solutionName)
        {
            var result = new List<CSharpIndexerResult>();
            SyntaxTree synTree2 = null;
            document.TryGetSyntaxTree(out synTree2);
            var root2 = (CompilationUnitSyntax)synTree2.GetRoot();
            var nodes = root2.DescendantNodesAndSelf();
            var argumentNodes = nodes
                .OfType<ArgumentListSyntax>()
                .Where(x => x.Arguments.Any())
                .Select(x => x)
                .ToList();
            foreach (var n in argumentNodes)
            {
                if (n is ArgumentListSyntax)
                {
                    var t = n.Ancestors().ToList();
                    if (t.Any(x =>
                         semModel2.GetSymbolInfo(x).Symbol != null &&
                         semModel2.GetSymbolInfo(x).Symbol.ContainingNamespace.ToDisplayString().Contains("Batis")
                    ))
                    {
                        Location loc = Location.Create(synTree2, n.Span);

                        string projectName = fileInfo.ProjectName;
                        var splitted = fileInfo.FilePath.Split('\\').ToList();
                        var projectNameIndex = splitted.LastIndexOf(projectName);
                        var projectFilePath = splitted.Skip(projectNameIndex + 1);
                        string relativePath = $"{solutionName}\\{projectName}\\{string.Join("\\", projectFilePath)}";
                        CodeStatmentInfo csInfo = new CodeStatmentInfo
                        {
                            LineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            RelativePath = $"{relativePath.Replace(@"\\", @"\")}",
                            StatmentFile = Path.GetFileName(fileInfo.FilePath),
                        };
                        IndexerKey key = IndexerKey.ConvertToKey(n.Arguments.FirstOrDefault().ToString().Replace("\"", "").Trim(), document.Project.Name);
                        result.Add(new CSharpIndexerResult
                        {
                            QueryFileName = Path.GetFileName(fileInfo.FilePath),
                            QueryId = n.Arguments.FirstOrDefault().ToString().Replace("\"", "").Trim(),
                            QueryLineNumber = loc.GetLineSpan().StartLinePosition.Line + 1,
                            QueryVsProjectName = document.Project.Name,
                            QueryFilePath = fileInfo.FilePath,
                        });
                    }
                }
            }
            return result;
        }
    }
}
