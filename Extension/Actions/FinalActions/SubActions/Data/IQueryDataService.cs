using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Storage.Providers;
using BatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace BatisSuperHelper.Actions.FinalActions.SubActions.Data
{
    public interface IQueryDataService<T> where T: BaseIndexerValue
    {
        List<IndexerKey> GetStatmentKeys(string query, bool useNamespace);
        List<T> GetStatmentsFromKeys(List<IndexerKey> keys);
        List<IndexerKey> GetStatmentKeysIgnoringNamespace(string query);
        List<ExpressionResult> GetResultsForGenericQueries(string queryResult, bool useNamespace);
        List<ResultWindowViewModel> PrepareViewModels(List<ExpressionResult> genericResults, ExpressionResult expressionResult, List<T> nonGenericResults);
        void Rename(IndexerKey key, string value);
    }
}
