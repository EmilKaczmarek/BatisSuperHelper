using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Data
{
    public class CodeQueryDataService : IQueryDataService<CSharpQuery>
    {
        public List<ExpressionResult> GetResultsForGenericQueries(string queryResult, NamespaceHandlingType namespaceHandlingLogic)
        {
            var genericResult = new List<ExpressionResult>();
            if (namespaceHandlingLogic == NamespaceHandlingType.WITH_NAMESPACE || namespaceHandlingLogic == NamespaceHandlingType.HYBRID_NAMESPACE)
                genericResult.AddRange(PackageStorage.GenericMethods.GetByPredictate(e => e.TextResult == queryResult));

            if (namespaceHandlingLogic == NamespaceHandlingType.IGNORE_NAMESPACE || namespaceHandlingLogic == NamespaceHandlingType.HYBRID_NAMESPACE)
                genericResult.AddRange(PackageStorage.GenericMethods.GetByPredictate(e => MapNamespaceHelper.GetQueryWithoutNamespace(e.TextResult) == MapNamespaceHelper.GetQueryWithoutNamespace(queryResult)));

            return genericResult;
        }

        public List<KeyValuePair<IndexerKey, List<CSharpQuery>>> GetKeyStatmentPairs(string queryResult, NamespaceHandlingType namespaceHandlingLogicType)
        {
            var keys = GetStatmentKeys(queryResult, namespaceHandlingLogicType);
            return keys.Select(e => new KeyValuePair<IndexerKey, List<CSharpQuery>>(e, GetSingleStatmentFromKey(e))).ToList();
        }

        public List<IndexerKey> GetStatmentKeys(string queryResult, NamespaceHandlingType namespaceHandlingLogicType)
        {
            return PackageStorage.CodeQueries.GetKeysByQueryId(queryResult, namespaceHandlingLogicType);
        }

        public List<CSharpQuery> GetStatmentsFromKeys(List<IndexerKey> keys)
        {
            return keys.Select(PackageStorage.CodeQueries.GetValue).SelectMany(x => x).ToList();
        }

        public List<CSharpQuery> GetSingleStatmentFromKey(IndexerKey key)
        {
            return PackageStorage.CodeQueries.GetValueOrNull(key);
        }

        public List<ResultWindowViewModel> PrepareViewModels(List<ExpressionResult> genericResults, ExpressionResult expressionResult, List<CSharpQuery> nonGenericResults)
        {
            return genericResults.Select(x => new ResultWindowViewModel
            {
                File = x.NodeInformation.FileName,
                Line = x.NodeInformation.LineNumber,
                FilePath = x.NodeInformation.FilePath,
                Query = x.TextResult,
                Namespace = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(expressionResult.TextResult).Item1,
            })
           .Concat(nonGenericResults.Select(x => new ResultWindowViewModel
           {
               File = x.QueryFileName,
               Line = x.QueryLineNumber,
               Query = x.QueryId,
               FilePath = x.QueryFilePath,
               Namespace = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(x.QueryId).Item1,
           })).ToList();
        }

        public void Rename(IndexerKey key, string value)
        {
            PackageStorage.CodeQueries.RenameQuery(key, value);
        }
    }
}
