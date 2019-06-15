using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Storage;
using BatisSuperHelper.Storage.Providers;
using BatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace BatisSuperHelper.Actions.FinalActions.SubActions.Data
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

        public List<IndexerKey> GetStatmentKeys(string query, bool useNamespace)
        {
            return GotoAsyncPackage.Storage.CodeQueries.GetKeys(query, useNamespace);
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
