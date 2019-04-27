using Microsoft.CodeAnalysis;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Storage.Domain;
using IBatisSuperHelper.Storage.Interfaces;
using Microsoft.VisualStudio.Shell;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Indexers.Code;

namespace IBatisSuperHelper.Storage
{
    public static class PackageStorage
    {
        public static IProvider<IndexerKey, List<CSharpQuery>> CodeQueries = new CodeQueryProvider();
        public static IProvider<IndexerKey, XmlQuery> XmlQueries = new XmlQueryProvider();

        public static XmlIndexer XmlFileAnalyzer = new XmlIndexer();
        public static CSharpIndexer CodeFileAnalyzer = new CSharpIndexer();

        public static GenericStorage<MethodInfo, ExpressionResult> GenericMethods = new GenericStorage<MethodInfo, ExpressionResult>();

        public static readonly GenericStorage<string, object> RuntimeConfiguration = new GenericStorage<string, object>
        {
            new KeyValuePair<string, object>("HybridNamespaceEnabled", true),
        };

        public static Settings BatisSettings { get; private set; }

        public static void SetBatisSettings(Settings batisSettings)
        {
            BatisSettings = batisSettings;
        }

        public static async System.Threading.Tasks.Task AnalyzeAndStoreAsync(List<Document> documents)
        {
            using (MiniProfiler.Current.Step(nameof(AnalyzeAndStoreAsync)))
            {
                var codeResults = await CodeFileAnalyzer.BuildIndexerAsync(documents);
                var generics = codeResults.SelectMany(e => e.Generics).Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e));
                await GenericMethods.AddMultipleAsync(generics);
                CodeQueries.AddMultipleWithoutKey(codeResults.Select(e => e.Queries).ToList());
            }
        }

        public static void AnalyzeAndStoreSingle(XmlFileInfo xmlFile)
        {
            var xmlFileResult = XmlFileAnalyzer.ParseSingleFile(xmlFile);
            XmlQueries.AddMultipleWithoutKey(xmlFileResult);
        }

        public static void AnalyzeAndUpdateSingle(Document document)
        {
            var codeResult = ThreadHelper.JoinableTaskFactory.Run(async () => await CodeFileAnalyzer.BuildFromDocumentAsync(document));
            CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { codeResult.Queries });
            foreach (var generic in codeResult.Generics)
            {
                GenericMethods.Update(generic.NodeInformation.MethodInfo, generic);
            }  
        }

        public static async System.Threading.Tasks.Task AnalyzeAndStoreSingleAsync(Document document)
        {
            using (MiniProfiler.Current.Step(nameof(AnalyzeAndStoreSingleAsync)))
            {
                var codeResult = await CodeFileAnalyzer.BuildAsync(document);
                await GenericMethods.AddMultipleAsync(codeResult.Generics.Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e)));
                CodeQueries.AddWithoutKey(codeResult.Queries);
            }
        }
    }
}
