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
    public class PackageStorage : IPackageStorage
    {
        public IProvider<IndexerKey, List<CSharpQuery>> CodeQueries { get; private set; }
        public IProvider<IndexerKey, XmlQuery> XmlQueries { get; private set; }

        public XmlIndexer XmlFileAnalyzer { get; private set; }
        public CSharpIndexer CodeFileAnalyzer { get; private set; }

        public GenericStorage<MethodInfo, ExpressionResult> GenericMethods { get; private set; }

        public GenericStorage<string, object> RuntimeConfiguration => new GenericStorage<string, object>
        {
            new KeyValuePair<string, object>("HybridNamespaceEnabled", true),
        };

        public PackageStorage()
        {
            CodeQueries = new CodeQueryProvider();
            XmlQueries = new XmlQueryProvider();
            XmlFileAnalyzer = new XmlIndexer();
            CodeFileAnalyzer = new CSharpIndexer();
            GenericMethods = new GenericStorage<MethodInfo, ExpressionResult>();
            //RuntimeConfiguration = runtimeConfiguration;
            BatisSettings = new Settings();
        }

        public PackageStorage(IProvider<IndexerKey, List<CSharpQuery>> codeQueries, IProvider<IndexerKey, XmlQuery> xmlQueries, XmlIndexer xmlFileAnalyzer, CSharpIndexer codeFileAnalyzer, GenericStorage<MethodInfo, ExpressionResult> genericMethods, GenericStorage<string, object> runtimeConfiguration, Settings batisSettings)
        {
            CodeQueries = codeQueries;
            XmlQueries = xmlQueries;
            XmlFileAnalyzer = xmlFileAnalyzer;
            CodeFileAnalyzer = codeFileAnalyzer;
            GenericMethods = genericMethods;
            //RuntimeConfiguration = runtimeConfiguration;
            BatisSettings = batisSettings;
        }

        public Settings BatisSettings { get; private set; }

        public void SetBatisSettings(Settings batisSettings)
        {
            BatisSettings = batisSettings;
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
