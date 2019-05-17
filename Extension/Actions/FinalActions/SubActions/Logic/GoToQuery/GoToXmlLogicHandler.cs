using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.VSIntegration;
using IBatisSuperHelper.Parsers.Models.SqlMap;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Logic
{
    public class GoToXmlLogicHandler : GoToBaseLogicHandler<Statement>
    {
        public GoToXmlLogicHandler(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane) : base(statusBar, toolWindowPane)
        {
        }

        public override string DetermineProperQueryText(string queryResult, ExpressionResult expressionResult)
        {
            return expressionResult.IsSolved? expressionResult.TextResult: queryResult;
        }

        public override void ShowText(int nonGenericKeysCount, string queryResult, bool shouldBeTerminated)
        {
            if (nonGenericKeysCount == 0 && !shouldBeTerminated)
                StatusBar.ShowText($"No occurence of query named: {queryResult} find in SqlMaps.");
            if (nonGenericKeysCount > 1 && !shouldBeTerminated)
                StatusBar.ShowText($"Multiple occurence of same statment({queryResult}) found.");
            if (shouldBeTerminated)
                StatusBar.ShowText($"Can not resolve expression.");
        }   
    }
}
