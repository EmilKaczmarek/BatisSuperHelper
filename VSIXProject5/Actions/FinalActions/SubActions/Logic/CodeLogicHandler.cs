using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers.Models;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions.FinalActions.SubActions.Logic
{
    public class CodeLogicHandler : BaseLogicHandler<CSharpQuery>
    {
        public CodeLogicHandler(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane) : base(statusBar, toolWindowPane)
        {
        }

        public override string DetermineProperQueryText(string queryResult, ExpressionResult expressionResult)
        {
            return queryResult;
        }

        public override bool ShouldBeTerminated(string queryResult, ExpressionResult expressionResult)
        {
            return !(expressionResult.IsSolved || (expressionResult.CanBeUsedAsQuery && !string.IsNullOrEmpty(queryResult)));
        }

        public override void ShowText(int nonGenericKeysCount, string queryResult, bool shouldBeTerminated)
        {
            if (nonGenericKeysCount == 0)
                StatusBar.ShowText($"No occurence of query named: {queryResult} find in Code.");
            if (nonGenericKeysCount > 1)
                StatusBar.ShowText($"Multiple occurence of same statment({queryResult}) found.");
        }
    }
}
