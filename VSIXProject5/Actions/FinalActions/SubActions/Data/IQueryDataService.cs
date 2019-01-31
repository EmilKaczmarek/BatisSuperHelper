using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage.Providers;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions.FinalActions.SubActions.Data
{
    public interface IQueryDataService<T> where T: BaseIndexerValue
    {
        List<IndexerKey> GetStatmentKeys(string query, NamespaceHandlingType namespaceHandlingLogic);
        List<T> GetStatmentsFromKeys(List<IndexerKey> keys);
        List<ExpressionResult> GetResultsForGenericQueries(string queryResult, NamespaceHandlingType namespaceHandlingLogic);
        List<ResultWindowViewModel> PrepareViewModels(List<ExpressionResult> genericResults, ExpressionResult expressionResult, List<T> nonGenericResults);
    }
}
