using Microsoft.VisualStudio.Shell;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Data;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.VSIntegration;
using EnvDTE80;
using Microsoft.VisualStudio.LanguageServices;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Logic.Rename;
using IBatisSuperHelper.Parsers.Models.SqlMap;

namespace IBatisSuperHelper.Actions.FinalActions.Factory
{
    public class CSharpFinalActionFactory : IFinalActionFactory
    {
        public GoToQueryFinalEventActionsExecutor GetFinalGoToQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            var useNamespace = GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces;
            return GoToQueryFinalEventActionsExecutor
                .Create()
                .WithLogicHandler(typeof(Statement), new GoToXmlLogicHandler(statusBar, toolWindowPane))
                .WithQueryDataService(typeof(Statement), new XmlQueryDataService())
                .WithUseNamespace(useNamespace);
        }

        public RenameFinalActionsExecutor GetFinalRenameQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane, DTE2 dte, VisualStudioWorkspace workspace)
        {
            return RenameFinalActionsExecutor
               .Create()
               .WithQueryDataService(typeof(CSharpQuery), new CodeQueryDataService())
               .WithQueryDataService(typeof(Statement), new XmlQueryDataService())
               .WithCodeLogicHandler(new RenameCodeLogicHandler())
               .WithXmlLogicHandler(new RenameXmlLogicHandler())
               .WithEnvDte(dte)
               .WithWorkspace(workspace);
        }
    }
}
