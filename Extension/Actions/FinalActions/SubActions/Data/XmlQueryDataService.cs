using System.Collections.Generic;
using System.Linq;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Parsers.Models.SqlMap;
using IBatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Data
{
    public class XmlQueryDataService : IQueryDataService<Statement>
    {
        public List<ExpressionResult> GetResultsForGenericQueries(string queryResult, bool useNamespace)
        {
            return new List<ExpressionResult>();
        }

        public List<IndexerKey> GetStatmentKeys(string query, bool useNamespace)
        {
            return GotoAsyncPackage.Storage.XmlQueries.GetKeysByQueryId(query, useNamespace);
        }

        public List<Statement> GetStatmentsFromKeys(List<IndexerKey> keys)
        {
            return keys.Select(GotoAsyncPackage.Storage.XmlQueries.GetValueOrNull).ToList();
        }

        public Statement GetSingleStatmentFromKey(IndexerKey key)
        {
            return GotoAsyncPackage.Storage.XmlQueries.GetValueOrNull(key);
        }

        public List<ResultWindowViewModel> PrepareViewModels(List<ExpressionResult> genericResults, ExpressionResult expressionResult, List<Statement> nonGenericResults)
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

        public void Rename(IndexerKey key, string value)
        {
            GotoAsyncPackage.Storage.XmlQueries.RenameQuery(key, value);
        }
    }
}
