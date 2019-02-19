using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Data;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage.Providers;

namespace IBatisSuperHelper.Actions.FinalActions
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
                var genericQueries = queryDataService.GetResultsForGenericQueries(queryResult, NamespaceHandlingLogicType);

                var keys = queryDataService.GetStatmentKeys(queryResult, NamespaceHandlingLogicType);
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
