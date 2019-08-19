using Microsoft.CodeAnalysis;
using StackExchange.Profiling;
using System.Collections.Generic;
using System.Linq;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Models;
using BatisSuperHelper.Storage.Domain;
using BatisSuperHelper.Storage.Interfaces;
using Microsoft.VisualStudio.Shell;
using BatisSuperHelper.Parsers.XmlConfig.Models;
using BatisSuperHelper.Indexers.Xml;
using BatisSuperHelper.Indexers.Code;
using BatisSuperHelper.Parsers.Models.XmlConfig.SqlMap;
using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Storage.Providers;
using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Storage.Event;
using System.Diagnostics;
using BatisSuperHelper.Parsers.Models.SqlMap;
using System;

namespace BatisSuperHelper.Storage
{
    public class PackageStorage : IPackageStorage
    {
        public long Initialized { get; private set; }
        public IQueryProvider<IndexerKey, List<CSharpQuery>> CodeQueries { get; private set; }
        public IQueryProvider<IndexerKey, Statement> XmlQueries { get; private set; }

        public XmlIndexer XmlFileAnalyzer { get; private set; }
        public CSharpIndexer CodeFileAnalyzer { get; private set; }

        public GenericStorage<MethodInfo, ExpressionResult> GenericMethods { get; private set; }

        public ISqlMapConfigProvider SqlMapConfigProvider { get; private set; }

        public GenericStorage<string, object> RuntimeConfiguration { get; private set; }

        public IndexingWorkflowOptions IndexingWorkflowOptions { get; private set; }

        public event EventHandler<StoreChangeEventArgs> OnStoreChange;

        public PackageStorage()
        {
            Initialized = DateTime.Now.Ticks;
            CodeQueries = new CodeQueryProvider();
            XmlQueries = new StatementProvider();
            XmlFileAnalyzer = new XmlIndexer();
            CodeFileAnalyzer = new CSharpIndexer();
            GenericMethods = new GenericStorage<MethodInfo, ExpressionResult>();
            SqlMapConfigProvider = new SqlMapConfigProvider();
            IndexingWorkflowOptions = new IndexingWorkflowOptions
            {
                MapsOptions = new SqlMapIndexingOptions
                {
                    IndexOnlyMapsInConfig = true,
                    IndexAllMapsOnError = true,
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

        private void OnStoreChangeHandler(ChangedFileTypeFlag flag)
        {
            StorageEvents.TriggerStoreChange(this, new StoreChangeEventArgs(flag));
        }

        public void Clear()
        {
            CodeQueries.Clear();
            XmlQueries.Clear();
            GenericMethods.Clear();
        }

        public async System.Threading.Tasks.Task AnalyzeAndStoreAsync(List<Document> documents)
        {
            using (MiniProfiler.Current.Step(nameof(AnalyzeAndStoreAsync)))
            {
                var codeResults = await CodeFileAnalyzer.BuildIndexerAsync(documents);
                var generics = codeResults.SelectMany(e => e.Generics).Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e));
                await GenericMethods.AddMultipleAsync(generics.Where(e => e.Value.CanBeUsedAsQuery));
                CodeQueries.AddMultipleWithoutKey(codeResults.Select(e => e.Queries).ToList());
                OnStoreChangeHandler(ChangedFileTypeFlag.CSharp);
            }
        }

        public void AnalyzeAndStoreSingle(XmlFileInfo xmlFile)
        {
            var xmlFileResult = XmlFileAnalyzer.ParseSingleFile(xmlFile);
            XmlQueries.AddMultipleWithoutKey(xmlFileResult);
            OnStoreChangeHandler(ChangedFileTypeFlag.Xml);
        }

        public void AnalyzeAndUpdateSingle(Document document)
        {
            var codeResult = ThreadHelper.JoinableTaskFactory.Run(async () => await CodeFileAnalyzer.BuildFromDocumentAsync(document));
            CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { codeResult.Queries });
            foreach (var generic in codeResult.Generics)
            {
                GenericMethods.Update(generic.NodeInformation.MethodInfo, generic);
            }
            OnStoreChangeHandler(ChangedFileTypeFlag.CSharp);
        }

        public async System.Threading.Tasks.Task AnalyzeAndUpdateSingleAsync(Document document)
        {
            var codeResult = await CodeFileAnalyzer.BuildFromDocumentAsync(document);
            CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { codeResult.Queries });
            foreach (var generic in codeResult.Generics)
            {
                await GenericMethods.UpdateAsync(generic.NodeInformation.MethodInfo, generic);
            }
            OnStoreChangeHandler(ChangedFileTypeFlag.CSharp);
        }

        public async System.Threading.Tasks.Task AnalyzeAndStoreSingleAsync(Document document)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.WriteLine($"AnalyzeAndStoreSingleAsync for file {document.Name}");
            using (MiniProfiler.Current.Step(nameof(AnalyzeAndStoreSingleAsync)))
            {
                var codeResult = await CodeFileAnalyzer.BuildAsync(document);
                await GenericMethods.AddMultipleAsync(codeResult.Generics.Select(e => new KeyValuePair<MethodInfo, ExpressionResult>(e.NodeInformation.MethodInfo, e)));
                CodeQueries.AddWithoutKey(codeResult.Queries);
                OnStoreChangeHandler(ChangedFileTypeFlag.CSharp);
            }
        }

    }
}
