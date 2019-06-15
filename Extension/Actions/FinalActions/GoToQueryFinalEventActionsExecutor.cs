using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Actions.FinalActions.SubActions.Data;
using BatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Storage.Providers;

namespace BatisSuperHelper.Actions.FinalActions
{
    public class GoToQueryFinalEventActionsExecutor : BaseFinalEventActionsExecutor<GoToQueryFinalEventActionsExecutor>
    {
        public static GoToQueryFinalEventActionsExecutor Create() => new GoToQueryFinalEventActionsExecutor();

        public override void Execute(string queryResult, ExpressionResult expressionResult)
        {
            dynamic queryDataService = QueryDataServices.FirstOrDefault().Value;
            dynamic logicHandler = LogicHandlers.FirstOrDefault().Value;

            int keysCount = 0;
            bool shouldBeTerminated = logicHandler.ShouldBeTerminated(queryResult, expressionResult);
            if (!shouldBeTerminated)
            {
                var genericQueries = queryDataService.GetResultsForGenericQueries(queryResult, UseNamespace);

                var keys = queryDataService.GetStatmentKeys(queryResult, UseNamespace);
                keysCount = keys.Count;

                var statments = queryDataService.GetStatmentsFromKeys(keys);

                var viewModels = queryDataService.PrepareViewModels(genericQueries, expressionResult, statments);

                logicHandler.NavigateToDocument(statments);
                logicHandler.ShowResults(viewModels);
            }
            logicHandler.ShowText(keysCount, queryResult, shouldBeTerminated);
        }
    }
}
