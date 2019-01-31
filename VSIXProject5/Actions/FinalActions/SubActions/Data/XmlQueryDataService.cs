using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage;
using VSIXProject5.Storage.Providers;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions.FinalActions.SubActions.Data
{
    public class XmlQueryDataService : IQueryDataService<XmlQuery>
    {
        public List<ExpressionResult> GetResultsForGenericQueries(string queryResult, NamespaceHandlingType namespaceHandlingLogic)
        {
            return new List<ExpressionResult>();
        }

        public List<IndexerKey> GetStatmentKeys(string query, NamespaceHandlingType namespaceHandlingLogic)
        {
            return PackageStorage.XmlQueries.GetKeysByQueryId(query, namespaceHandlingLogic);
        }

        public List<XmlQuery> GetStatmentsFromKeys(List<IndexerKey> keys)
        {
            return keys.Select(PackageStorage.XmlQueries.GetValueOrNull).ToList();
        }

        public List<ResultWindowViewModel> PrepareViewModels(List<ExpressionResult> genericResults, ExpressionResult expressionResult, List<XmlQuery> nonGenericResults)
        {
            return nonGenericResults.Select(x => new ResultWindowViewModel
           {
               File = x.QueryFileName,
               FilePath = x.QueryFilePath,
               Line = x.QueryLineNumber,
               Namespace = x.MapNamespace,
               Query = x.QueryId,
           }).ToList();
        }
    }
}
