using Microsoft.VisualStudio.Shell;
using BatisSuperHelper.Actions.FinalActions.SubActions.Data;
using BatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.VSIntegration;
using EnvDTE80;
using Microsoft.VisualStudio.LanguageServices;

namespace BatisSuperHelper.Actions.FinalActions.Factory
{
    public class XmlFinalActionFactory : IFinalActionFactory
    {
        public GoToQueryFinalEventActionsExecutor GetFinalGoToQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane, bool useNamespace)
        {
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
