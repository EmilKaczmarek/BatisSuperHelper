using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Loggers;
using VSIXProject5.Storage;
using VSIXProject5.Storage.Providers;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions2.FinalActions
{
    public class CSharpDefaultFinalActions : DefaultFinalActions, IFinalAction
    {
        private StatusBarIntegration _statusBar;

        public CSharpDefaultFinalActions(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane) : base(statusBar, toolWindowPane)
        {
            _statusBar = statusBar;
        }

        new public void ExecuteNoQueryAtLineFound()
        {
            base.ExecuteNoQueryAtLineFound();
        }

        new public void ExecuteRename()
        {
            base.ExecuteRename();
        }

        new public void PrepareAndExecuteGoToQuery(string queryResult, ExpressionResult expressionResult)
        {
            if (expressionResult.IsSolved || (expressionResult.CanBeUsedAsQuery && !string.IsNullOrEmpty(queryResult)))
            {
                var queryText = expressionResult.IsSolved ? expressionResult.TextResult : queryResult;
                List <ResultWindowViewModel> windowViewModels = new List<ResultWindowViewModel>();
                NamespaceHandlingType namspaceHandlingLogic =
                    (bool)PackageStorage.RuntimeConfiguration.GetValue("HybridNamespaceEnabled")
                    ? NamespaceHandlingType.HYBRID_NAMESPACE
                    : NamespaceHandlingType.WITH_NAMESPACE;

                var keys = PackageStorage.XmlQueries.GetKeysByQueryId(queryText, namspaceHandlingLogic);
                if (keys.Any())
                {
                    var statment = PackageStorage.XmlQueries.GetValueOrNull(keys.First());
                    windowViewModels = keys.Select(PackageStorage.XmlQueries.GetValueOrNull).Select(x => new ResultWindowViewModel
                    {
                        File = x.QueryFileName,
                        FilePath = x.QueryFilePath,
                        Line = x.QueryLineNumber,
                        Namespace = x.MapNamespace,
                        Query = x.QueryId,
                    }).ToList();

                    DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statment.QueryFilePath, statment.QueryLineNumber);
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryText} found in SqlMaps.");
                }
            }     
            else
            {
                _statusBar.ShowText($"Can't resolve expression.");
                OutputWindowLogger.WriteLn($"Unable to resolve {expressionResult.ExpressionText}. Reason: {expressionResult.UnresolvableReason}");
            }
        }
    }
}
