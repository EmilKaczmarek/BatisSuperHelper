using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage;
using VSIXProject5.Storage.Providers;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions.FinalActions.SubActions.Data
{
    public class CodeQueryDataService<T> : IQueryDataService<CSharpQuery> where T: BaseIndexerValue
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

        public List<IndexerKey> GetStatmentKeys(string queryResult, NamespaceHandlingType namespaceHandlingLogic)
        {
            return PackageStorage.CodeQueries.GetKeysByQueryId(queryResult, namespaceHandlingLogic);
        }

        public List<CSharpQuery> GetStatmentsFromKeys(List<IndexerKey> keys)
        {
            return keys.Select(PackageStorage.CodeQueries.GetValue).SelectMany(x => x).ToList();
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
    }
}
