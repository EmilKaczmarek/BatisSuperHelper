using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Data
{
    public interface IQueryDataService<T> where T: BaseIndexerValue
    {
        List<IndexerKey> GetStatmentKeys(string query, NamespaceHandlingType namespaceHandlingLogic);
        List<T> GetStatmentsFromKeys(List<IndexerKey> keys);
        List<ExpressionResult> GetResultsForGenericQueries(string queryResult, NamespaceHandlingType namespaceHandlingLogic);
        List<ResultWindowViewModel> PrepareViewModels(List<ExpressionResult> genericResults, ExpressionResult expressionResult, List<T> nonGenericResults);
    }
}
