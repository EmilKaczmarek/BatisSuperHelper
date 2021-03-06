﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using StackExchange.Profiling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BatisSuperHelper.Constants;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.HelpersAndExtensions.Roslyn;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Models;
using BatisSuperHelper.Constants.BatisConstants;

namespace BatisSuperHelper.Indexers.Code
{
    public class CSharpIndexer
    {
        public object CodeCostants { get; private set; }

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

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            OutputWindowLogger.WriteLn($"Building Queries db from code ended in {sw.ElapsedMilliseconds} ms. Found {results.Count} queries. In {documents.Count} documents.");
            return results;
        }

        public async Task<CSharpIndexerResult> BuildFromDocumentAsync(Document document)
        {
            using (MiniProfiler.Current.Step(nameof(BuildFromDocumentAsync)))
            {
                return await BuildAsync(document);
            }

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

        public async Task<CSharpIndexerResult> BuildAsync(Document document)
        {
            using (var timing = MiniProfiler.Current.Step(nameof(BuildAsync)))
            {
                return Build(await document.GetSemanticModelAsync(), document);
            }
        }

        private CSharpIndexerResult Build(SemanticModel semModel, Document document)
        {
            using (MiniProfiler.Current.Step(nameof(Build)))
            {
                var queryResults = new List<CSharpQuery>();
                var genericResults = new List<ExpressionResult>();
                document.TryGetSyntaxTree(out SyntaxTree synTree);
                var treeRoot = (CompilationUnitSyntax)synTree.GetRoot();
                IEnumerable<SyntaxNode> nodes;
                IEnumerable<ArgumentListSyntax> argumentNodes;
                using (MiniProfiler.Current.Step("DescendantNodes and .OfType().Where()"))
                {
                    nodes = treeRoot.DescendantNodesAndSelf();
                    argumentNodes = nodes
                        .OfType<ArgumentListSyntax>()
                        .Where(x => x.Arguments.Any());
                }
                using (MiniProfiler.Current.Step("foreach"))
                {
                    foreach (var node in argumentNodes)
                    {
                        var firstInvocationOfNodeAncestors = node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();

                        if (firstInvocationOfNodeAncestors == null)
                            continue;

                        var allowedTypes = new List<Type> { typeof(IdentifierNameSyntax), typeof(GenericNameSyntax) };
                        IEnumerable<SimpleNameSyntax> nameIdentifiers;
                        using (MiniProfiler.Current.Step("nameIdentifiers"))
                        {
                            nameIdentifiers = firstInvocationOfNodeAncestors.Expression.DescendantNodes().Where(e => allowedTypes.Contains(e.GetType())).Cast<SimpleNameSyntax>();
                        }
                        if (nameIdentifiers.Any(e => CodeConstants.MethodNames.Contains(e.Identifier.ValueText)))
                        {
                            Location loc = Location.Create(synTree, node.Span);
                            ExpressionResult expressionResult;
                            using (MiniProfiler.Current.Step("expression resolver"))
                            {
                                expressionResult = new ExpressionResolver().GetStringValueOfExpression(document, node.Arguments.FirstOrDefault().Expression, nodes, semModel);
                            }
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
    }
}
