using Microsoft.CodeAnalysis;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Storage.Domain;
using IBatisSuperHelper.Storage.Interfaces;

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
        
        public static async Task AnalyzeAndStoreAsync(List<Document> documents)
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

        public static async Task AnalyzeAndStoreSingleAsync(Document document)
        {
            var codeResult = await CodeFileAnalyzer.BuildFromDocumentAsync(document);
            await GenericMethods.AddMultipleAsync(codeResult.Generics.Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e)));
            CodeQueries.AddWithoutKey(codeResult.Queries);
        }

        public static void AnalyzeAndUpdateSingle(Document document)
        {
            var codeResult = CodeFileAnalyzer.BuildFromDocumentAsync(document).Result;
            CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { codeResult.Queries });
            foreach (var generic in codeResult.Generics)
            {
                GenericMethods.Update(generic.NodeInformation.MethodInfo, generic);
            }  
        }
    }
}
