using Microsoft.VisualStudio.Shell;
using BatisSuperHelper.Actions.FinalActions.SubActions.Data;
using BatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Storage;
using BatisSuperHelper.Storage.Providers;
using BatisSuperHelper.VSIntegration;
using EnvDTE80;
using Microsoft.VisualStudio.LanguageServices;
using BatisSuperHelper.Actions.FinalActions.SubActions.Logic.Rename;
using BatisSuperHelper.Parsers.Models.SqlMap;

namespace BatisSuperHelper.Actions.FinalActions.Factory
{
    public class CSharpFinalActionFactory : IFinalActionFactory
    {
        public GoToQueryFinalEventActionsExecutor GetFinalGoToQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane, bool useNamespace)
        {
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
