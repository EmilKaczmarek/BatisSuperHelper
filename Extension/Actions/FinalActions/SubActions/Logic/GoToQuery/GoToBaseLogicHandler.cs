using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.VSIntegration;
using BatisSuperHelper.VSIntegration.Navigation;
using BatisSuperHelper.Windows.ResultWindow.ViewModel;

namespace BatisSuperHelper.Actions.FinalActions.SubActions.Logic
{
    public abstract class GoToBaseLogicHandler<T> where T : BaseIndexerValue
    {
        protected StatusBarIntegration StatusBar;
        protected ToolWindowPane ToolWindowPane;

        private GoToBaseLogicHandler() { }

        protected GoToBaseLogicHandler(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            StatusBar = statusBar;
            ToolWindowPane = toolWindowPane;
        }

        public abstract string DetermineProperQueryText(string queryResult, ExpressionResult expressionResult);
        public abstract void ShowText(int nonGenericKeysCount, string queryResult, bool shouldBeTerminated);

        public virtual bool ShouldBeTerminated(string queryResult, ExpressionResult expressionResult)
        {
            return false;
        }

        public virtual void NavigateToDocument(List<T> nonGenericResults)
        {
            if (nonGenericResults != null && nonGenericResults.Count == 1)
                DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(nonGenericResults.First().QueryFilePath, nonGenericResults.First().QueryLineNumber);
        }

        public virtual void ShowResults(List<ResultWindowViewModel> windowViewModels)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (windowViewModels != null)
            {
                var windowContent = (ResultWindowControl)ToolWindowPane.Content;
                windowContent.ShowResults(windowViewModels);
                IVsWindowFrame windowFrame = (IVsWindowFrame)ToolWindowPane.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }
    }
}
