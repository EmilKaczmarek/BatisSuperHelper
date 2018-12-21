using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions2.FinalActions
{
    public class DefaultFinalActions : IFinalAction
    {
        private StatusBarIntegration _statusBar;
        private ToolWindowPane _toolWindowPane;

        public DefaultFinalActions(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            _statusBar = statusBar;
            _toolWindowPane = toolWindowPane;
        }

        public void PrepareAndExecuteGoToQuery(string queryResult, ExpressionResult expressionResult)
        {
            var statmentsKeys = PackageStorage.CodeQueries.GetKeysByQueryId(queryResult);
            var statments = statmentsKeys.Select(PackageStorage.CodeQueries.GetValue).SelectMany(x => x);

            List<ResultWindowViewModel> windowViewModels = new List<ResultWindowViewModel>();
            if (!statments.Any())
            {
                _statusBar.ShowText($"No occurence of query named: {queryResult} find in Code.");
            }
            if (statments.Count() == 1)
            {
                DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statments.First().QueryFilePath, statments.First().QueryLineNumber);
            }
            if (statments.Count() > 1)
            {
                windowViewModels = statments.Select(x => new ResultWindowViewModel
                {
                    File = x.QueryFileName,
                    Line = x.QueryLineNumber,
                    Query = x.QueryId,
                    FilePath = x.QueryFilePath,
                    Namespace = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(x.QueryId).Item1,
                }).ToList();

                _statusBar.ShowText($"Multiple occurence of same statment({queryResult}) found.");
            }

            if (windowViewModels != null && windowViewModels.Count > 1)
            {
                var windowContent = (ResultWindowControl)_toolWindowPane.Content;
                windowContent.ShowResults(windowViewModels);
                IVsWindowFrame windowFrame = (IVsWindowFrame)_toolWindowPane.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }

        public void ExecuteRename()
        {
            throw new NotImplementedException();
        }

        public void ExecuteNoQueryAtLineFound()
        {
            _statusBar.ShowText($"No query found in selected line.");
        }
    }
}
