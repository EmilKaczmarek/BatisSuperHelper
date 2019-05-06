using System.Collections.Generic;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Code;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Models;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using IBatisSuperHelper.Storage.Interfaces;
using Microsoft.CodeAnalysis;

namespace IBatisSuperHelper.Storage
{
    public interface IPackageStorage
    {
        Settings BatisSettings { get; }
        CSharpIndexer CodeFileAnalyzer { get; }
        IProvider<IndexerKey, List<CSharpQuery>> CodeQueries { get; }
        GenericStorage<MethodInfo, ExpressionResult> GenericMethods { get; }
        GenericStorage<string, object> RuntimeConfiguration { get; }
        XmlIndexer XmlFileAnalyzer { get; }
        IProvider<IndexerKey, XmlQuery> XmlQueries { get; }

        Task AnalyzeAndStoreAsync(List<Document> documents);
        void AnalyzeAndStoreSingle(XmlFileInfo xmlFile);
        Task AnalyzeAndStoreSingleAsync(Document document);
        void AnalyzeAndUpdateSingle(Document document);
        void SetBatisSettings(Settings batisSettings);
    }
}