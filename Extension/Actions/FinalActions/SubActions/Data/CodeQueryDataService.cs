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
        public List<ExpressionResult> GetResultsForGenericQueries(string queryResult, bool useNamespace)
        {
            return GotoAsyncPackage.Storage.GenericMethods.GetByPredictate(e => e.TextResult == queryResult).ToList();
        }

        public List<KeyValuePair<IndexerKey, List<CSharpQuery>>> GetKeyStatmentPairs(string queryResult, bool useNamespace)
        {
            var keys = GetStatmentKeys(queryResult, useNamespace);
            return keys.Select(e => new KeyValuePair<IndexerKey, List<CSharpQuery>>(e, GetSingleStatmentFromKey(e))).ToList();
        }

        public List<IndexerKey> GetStatmentKeys(string queryResult, bool useNamespace)
        {
            return GotoAsyncPackage.Storage.CodeQueries.GetKeysByQueryId(queryResult, useNamespace);
        }

        public List<CSharpQuery> GetStatmentsFromKeys(List<IndexerKey> keys)
        {
            return keys.Select(GotoAsyncPackage.Storage.CodeQueries.GetValue).SelectMany(x => x).ToList();
        }

        public List<CSharpQuery> GetSingleStatmentFromKey(IndexerKey key)
        {
            return GotoAsyncPackage.Storage.CodeQueries.GetValueOrNull(key);
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
            GotoAsyncPackage.Storage.CodeQueries.RenameQuery(key, value);
        }
    }
}
