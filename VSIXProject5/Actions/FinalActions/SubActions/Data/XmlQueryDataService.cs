using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Data
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
