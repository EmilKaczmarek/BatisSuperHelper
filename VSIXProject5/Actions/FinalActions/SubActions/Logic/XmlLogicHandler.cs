using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers.Models;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions.FinalActions.SubActions.Logic
{
    public class XmlLogicHandler : BaseLogicHandler<XmlQuery>
    {
        public XmlLogicHandler(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane) : base(statusBar, toolWindowPane)
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
