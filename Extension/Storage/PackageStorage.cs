using Microsoft.CodeAnalysis;
using StackExchange.Profiling;
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
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Indexers.Code;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Parsers.Models.SqlMap;

namespace IBatisSuperHelper.Storage
{
    public class PackageStorage : IPackageStorage
    {
        public IQueryProvider<IndexerKey, List<CSharpQuery>> CodeQueries { get; private set; }
        public IQueryProvider<IndexerKey, Statement> XmlQueries { get; private set; }

        public XmlIndexer XmlFileAnalyzer { get; private set; }
        public CSharpIndexer CodeFileAnalyzer { get; private set; }

        public GenericStorage<MethodInfo, ExpressionResult> GenericMethods { get; private set; }

        public ISqlMapConfigProvider SqlMapConfigProvider { get; private set; }

        public GenericStorage<string, object> RuntimeConfiguration { get; private set; }

        public IndexingWorkflowOptions IndexingWorkflowOptions { get; private set; }

        public PackageStorage()
        {
            CodeQueries = new CodeQueryProvider();
            XmlQueries = new StatementProvider();
            XmlFileAnalyzer = new XmlIndexer();
            CodeFileAnalyzer = new CSharpIndexer();
            GenericMethods = new GenericStorage<MethodInfo, ExpressionResult>();
            SqlMapConfigProvider = new SqlMapConfigProvider();
            IndexingWorkflowOptions = new IndexingWorkflowOptions
            {
                ConfigOptions = new ConfigsIndexingOptions
                {
                    SupportMultipleConfigs = true,
                },
                MapsOptions = new SqlMapIndexingOprions
                {
                    IndexAllMaps = true,
                },
            };
        }

        public PackageStorage(IQueryProvider<IndexerKey, List<CSharpQuery>> codeQueries, IQueryProvider<IndexerKey, Statement> xmlQueries, XmlIndexer xmlFileAnalyzer, CSharpIndexer codeFileAnalyzer, GenericStorage<MethodInfo, ExpressionResult> genericMethods, ISqlMapConfigProvider sqlMapConfigProvider, GenericStorage<string, object> runtimeConfiguration)
        {
            CodeQueries = codeQueries;
            XmlQueries = xmlQueries;
            XmlFileAnalyzer = xmlFileAnalyzer;
            CodeFileAnalyzer = codeFileAnalyzer;
            GenericMethods = genericMethods;
            SqlMapConfigProvider = sqlMapConfigProvider;
            RuntimeConfiguration = runtimeConfiguration;
        }

        public async System.Threading.Tasks.Task AnalyzeAndStoreAsync(List<Document> documents)
        {
            using (MiniProfiler.Current.Step(nameof(AnalyzeAndStoreAsync)))
            {
                var codeResults = await CodeFileAnalyzer.BuildIndexerAsync(documents);
                var generics = codeResults.SelectMany(e => e.Generics).Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e));
                await GenericMethods.AddMultipleAsync(generics);
                CodeQueries.AddMultipleWithoutKey(codeResults.Select(e => e.Queries).ToList());
            }
        }

        public void AnalyzeAndStoreSingle(XmlFileInfo xmlFile)
        {
            var xmlFileResult = XmlFileAnalyzer.ParseSingleFile(xmlFile);
            XmlQueries.AddMultipleWithoutKey(xmlFileResult);
        }

        public void AnalyzeAndUpdateSingle(Document document)
        {
            var codeResult = ThreadHelper.JoinableTaskFactory.Run(async () => await CodeFileAnalyzer.BuildFromDocumentAsync(document));
            CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { codeResult.Queries });
            foreach (var generic in codeResult.Generics)
            {
                GenericMethods.Update(generic.NodeInformation.MethodInfo, generic);
            }  
        }

        public async System.Threading.Tasks.Task AnalyzeAndUpdateSingleAsync(Document document)
        {
            var codeResult = await CodeFileAnalyzer.BuildFromDocumentAsync(document);
            CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { codeResult.Queries });
            foreach (var generic in codeResult.Generics)
            {
                await GenericMethods.UpdateAsync(generic.NodeInformation.MethodInfo, generic);
            }
        }

        public async System.Threading.Tasks.Task AnalyzeAndStoreSingleAsync(Document document)
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
