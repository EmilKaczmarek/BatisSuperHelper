using System.Collections.Generic;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Code;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Storage.Interfaces;
using IBatisSuperHelper.Storage.Providers;
using Microsoft.CodeAnalysis;

namespace IBatisSuperHelper.Storage
{
    public interface IPackageStorage
    {
        CSharpIndexer CodeFileAnalyzer { get; }
        IQueryProvider<IndexerKey, List<CSharpQuery>> CodeQueries { get; }
        GenericStorage<MethodInfo, ExpressionResult> GenericMethods { get; }
        GenericStorage<string, object> RuntimeConfiguration { get; }
        ISqlMapConfigProvider SqlMapConfigProvider { get; }
        XmlIndexer XmlFileAnalyzer { get; }
        IQueryProvider<IndexerKey, XmlQuery> XmlQueries { get; }
        IndexingWorkflowOptions IndexingWorkflowOptions { get; }

        Task AnalyzeAndStoreAsync(List<Document> documents);
        void AnalyzeAndStoreSingle(XmlFileInfo xmlFile);
        Task AnalyzeAndStoreSingleAsync(Document document);
        void AnalyzeAndUpdateSingle(Document document);
        Task AnalyzeAndUpdateSingleAsync(Document document);
    }
}