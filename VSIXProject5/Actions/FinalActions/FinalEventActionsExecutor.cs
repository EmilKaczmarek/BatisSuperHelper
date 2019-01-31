using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Actions.FinalActions.SubActions.Data;
using VSIXProject5.Actions.FinalActions.SubActions.Logic;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage.Providers;

namespace VSIXProject5.Actions.FinalActions
{
    public class FinalEventActionsExecutor
    {
        public static FinalEventActionsExecutor Create() => new FinalEventActionsExecutor();

        private dynamic _queryDataService;
        private dynamic _logicHandler;
        private NamespaceHandlingType _namespaceHandlingLogicType;

        private FinalEventActionsExecutor() { }

        public FinalEventActionsExecutor WithQueryDataService(dynamic queryDataService)
        {
            _queryDataService = queryDataService;
            return this;
        }

        public FinalEventActionsExecutor WithLogicHandler(dynamic logicHandler)
        {
            _logicHandler = logicHandler;
            return this;
        }


        public FinalEventActionsExecutor WithNamespaceHandlingLogicType(NamespaceHandlingType namespaceHandlingLogicType)
        {
            _namespaceHandlingLogicType = namespaceHandlingLogicType;
            return this;
        }

        public void Execute(string queryResult, ExpressionResult expressionResult)
        {
            int keysCount = 0;
            bool shouldBeTerminated = _logicHandler.ShouldBeTerminated(queryResult, expressionResult);
            if (!shouldBeTerminated)
            {
                var genericQueries = _queryDataService.GetResultsForGenericQueries(queryResult, _namespaceHandlingLogicType);

                var keys = _queryDataService.GetStatmentKeys(queryResult, _namespaceHandlingLogicType);
                keysCount = keys.Count;

                var statments = _queryDataService.GetStatmentsFromKeys(keys);

                var viewModels = _queryDataService.PrepareViewModels(genericQueries, expressionResult, statments);

                _logicHandler.NavigateToDocument(statments);
                _logicHandler.ShowResults(viewModels);
            }
            _logicHandler.ShowText(keysCount, queryResult, shouldBeTerminated);
        }
    }
}
