using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Models;
using VSIXProject5.Storage.Domain;
using VSIXProject5.Storage.Interfaces;

namespace VSIXProject5.Storage
{
    public static class PackageStorage
    {
        public static IProvider<IndexerKey, List<CSharpIndexerResult>> CodeQueries = new CodeQueryProvider();
        public static IProvider<IndexerKey, XmlIndexerResult> XmlQueries = new XmlQueryProvider();

        public static XmlIndexer XmlFileAnalyzer = new XmlIndexer();
        public static CSharpIndexer CodeFileAnalyzer = new CSharpIndexer();

        public static void AnalyzeAndStore(List<XmlFileInfo> xmlFiles)
        {
            var xmlResults = XmlFileAnalyzer.BuildIndexerAsync(xmlFiles);
            XmlQueries.AddMultipleWithoutKey(xmlResults);
        }

        public static void AnalyzeAndStore(List<Document> documents)
        {
            var codeResults = CodeFileAnalyzer.BuildIndexerAsync(documents).Result;
            CodeQueries.AddMultipleWithoutKey(codeResults);
        }

        public static async Task AnalyzeAndStoreAsync(List<Document> documents)
        {
            var codeResults = await CodeFileAnalyzer.BuildIndexerAsync(documents);
            CodeQueries.AddMultipleWithoutKey(codeResults);
        }

        public static void AnalyzeAndStoreSingle(XmlFileInfo xmlFile)
        {
            var xmlFileResult = XmlFileAnalyzer.ParseSingleFile(xmlFile);
            XmlQueries.AddMultipleWithoutKey(xmlFileResult);
        }

        public static void AnalyzeAndStoreSingle(Document document)
        {
            var codeResult = CodeFileAnalyzer.BuildFromDocumentAsync(document).Result;
            CodeQueries.AddWithoutKey(codeResult);
        }

        public static async Task AnalyzeAndStoreSingleAsync(Document document)
        {
            var codeResult = await CodeFileAnalyzer.BuildFromDocumentAsync(document);
            CodeQueries.AddWithoutKey(codeResult);
        }
    }
}
