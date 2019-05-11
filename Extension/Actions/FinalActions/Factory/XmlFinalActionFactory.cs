using Microsoft.VisualStudio.Shell;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Data;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.VSIntegration;
using EnvDTE80;
using Microsoft.VisualStudio.LanguageServices;

namespace IBatisSuperHelper.Actions.FinalActions.Factory
{
    public class XmlFinalActionFactory : IFinalActionFactory
    {
        public GoToQueryFinalEventActionsExecutor GetFinalGoToQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            var useNamespace = GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces;
            return GoToQueryFinalEventActionsExecutor
                .Create()
                .WithLogicHandler(typeof(CSharpQuery), new GoToCodeLogicHandler(statusBar, toolWindowPane))
                .WithQueryDataService(typeof(CSharpQuery), new CodeQueryDataService())
                .WithUseNamespace(useNamespace);
        }

        public RenameFinalActionsExecutor GetFinalRenameQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane, DTE2 dte, VisualStudioWorkspace workspace)
        {
            return null;
        }
    }
}
