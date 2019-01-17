using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Models;
using VSIXProject5.Storage.Domain;
using VSIXProject5.Storage.Interfaces;

namespace VSIXProject5.Storage
{
    public static class PackageStorage
    {
        public static IProvider<IndexerKey, List<CSharpQuery>> CodeQueries = new CodeQueryProvider();
        public static IProvider<IndexerKey, XmlQuery> XmlQueries = new XmlQueryProvider();

        public static XmlIndexer XmlFileAnalyzer = new XmlIndexer();
        public static CSharpIndexer CodeFileAnalyzer = new CSharpIndexer();

        public static GenericStorage<MethodInfo, ExpressionResult> GenericMethods = new GenericStorage<MethodInfo, ExpressionResult>();

        public static void AnalyzeAndStore(List<XmlFileInfo> xmlFiles)
        {
            var xmlResults = XmlFileAnalyzer.BuildIndexerAsync(xmlFiles);
            XmlQueries.AddMultipleWithoutKey(xmlResults);
        }

        public static void AnalyzeAndStore(List<Document> documents)
        {
            var codeResults = CodeFileAnalyzer.BuildIndexerAsync(documents).Result;
            CodeQueries.AddMultipleWithoutKey(codeResults.Select(e=>e.Queries).ToList());
            GenericMethods.AddMultiple(codeResults.SelectMany(e => e.Generics).Select(e=> new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e)));
        }

        public static async Task AnalyzeAndStoreAsync(List<Document> documents)
        {
            var codeResults = await CodeFileAnalyzer.BuildIndexerAsync(documents);
            var generics = codeResults.SelectMany(e => e.Generics).Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e));
            await GenericMethods.AddMultipleAsync(generics);
            CodeQueries.AddMultipleWithoutKey(codeResults.Select(e => e.Queries).ToList());
        }

        public static void AnalyzeAndStoreSingle(XmlFileInfo xmlFile)
        {
            var xmlFileResult = XmlFileAnalyzer.ParseSingleFile(xmlFile);
            XmlQueries.AddMultipleWithoutKey(xmlFileResult);
        }

        public static void AnalyzeAndStoreSingle(Document document)
        {
            var codeResult = CodeFileAnalyzer.BuildFromDocumentAsync(document).Result;
            CodeQueries.AddWithoutKey(codeResult.Queries);
            GenericMethods.AddMultiple(codeResult.Generics.Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e)));
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
