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
using VSIXProject5.Storage.Providers;
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
            var debugPackagageStorage = PackageStorage.GenericMethods.GetByPredictate(e => e.TextResult == queryResult).ToList();
            var debugPackagageStorage2 = PackageStorage.GenericMethods.GetByPredictate(e => MapNamespaceHelper.GetQueryWithoutNamespace(e.TextResult) == MapNamespaceHelper.GetQueryWithoutNamespace(queryResult)).ToList();

            NamespaceHandlingType namspaceHandlingLogic =
                (bool)PackageStorage.RuntimeConfiguration.GetValue("HybridNamespaceEnabled")
                ? NamespaceHandlingType.HYBRID_NAMESPACE
                : NamespaceHandlingType.WITH_NAMESPACE;

            var statmentsKeys = PackageStorage.CodeQueries.GetKeysByQueryId(queryResult, namspaceHandlingLogic);
            var statments = statmentsKeys.Select(PackageStorage.CodeQueries.GetValue).SelectMany(x => x);
        
            List<ResultWindowViewModel> windowViewModels = new List<ResultWindowViewModel>();
            if (!statments.Any())
            {
                _statusBar.ShowText($"No occurence of query named: {queryResult} find in Code.");
            }
            if (debugPackagageStorage2.Any())
            {
                windowViewModels.AddRange(debugPackagageStorage2.Select(x => new ResultWindowViewModel
                {
                    File = x.NodeInformation.FileName,
                    Line = x.NodeInformation.LineNumber,
                    FilePath = x.NodeInformation.FilePath,
                    Query = x.TextResult,
                    Namespace = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(expressionResult.TextResult).Item1,
                })); 
            }
            if (statments.Count() == 1)
            {
                DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statments.First().QueryFilePath, statments.First().QueryLineNumber);
            }
            if (statments.Count() > 1)
            {
                windowViewModels.AddRange(statments.Select(x => new ResultWindowViewModel
                {
                    File = x.QueryFileName,
                    Line = x.QueryLineNumber,
                    Query = x.QueryId,
                    FilePath = x.QueryFilePath,
                    Namespace = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(x.QueryId).Item1,
                }));

                _statusBar.ShowText($"Multiple occurence of same statment({queryResult}) found.");
            }

            if (windowViewModels != null)
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
