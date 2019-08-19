using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Code;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Indexers.Xml;
using BatisSuperHelper.Models;
using BatisSuperHelper.Storage.Interfaces;
using BatisSuperHelper.Storage.Providers;
using BatisSuperHelper.Parsers.Models.SqlMap;
using Microsoft.CodeAnalysis;

namespace BatisSuperHelper.Storage
{
    public interface IPackageStorage
    {
        long Initialized { get; }
        CSharpIndexer CodeFileAnalyzer { get; }
        IQueryProvider<IndexerKey, List<CSharpQuery>> CodeQueries { get; }
        GenericStorage<MethodInfo, ExpressionResult> GenericMethods { get; }
        GenericStorage<string, object> RuntimeConfiguration { get; }
        ISqlMapConfigProvider SqlMapConfigProvider { get; }
        XmlIndexer XmlFileAnalyzer { get; }
        IQueryProvider<IndexerKey, Statement> XmlQueries { get; }
        IndexingWorkflowOptions IndexingWorkflowOptions { get; }
        void Clear();
        Task AnalyzeAndStoreAsync(List<Document> documents);
        void AnalyzeAndStoreSingle(XmlFileInfo xmlFile);
        Task AnalyzeAndStoreSingleAsync(Document document);
        void AnalyzeAndUpdateSingle(Document document);
        Task AnalyzeAndUpdateSingleAsync(Document document);
    }
}