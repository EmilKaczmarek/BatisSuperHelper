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

namespace IBatisSuperHelper.Actions.FinalActions.Factory
{
    public class CSharpFinalActionFactory : IFinalActionFactory
    {
        public GoToQueryFinalEventActionsExecutor GetFinalGoToQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            var isHybridNamespaceHandlingEnabled = (bool)PackageStorage.RuntimeConfiguration.GetValue("HybridNamespaceEnabled");
            return GoToQueryFinalEventActionsExecutor
                .Create()
                .WithLogicHandler(typeof(XmlQuery), new GoToXmlLogicHandler(statusBar, toolWindowPane))
                .WithQueryDataService(typeof(XmlQuery), new XmlQueryDataService())
                .WithNamespaceHandlingLogicType(
                    isHybridNamespaceHandlingEnabled
                    ? NamespaceHandlingType.HYBRID_NAMESPACE
                    : NamespaceHandlingType.WITH_NAMESPACE);
        }

        public RenameFinalActionsExecutor GetFinalRenameQueryActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane, DTE2 dte, VisualStudioWorkspace workspace)
        {
            return RenameFinalActionsExecutor
               .Create()
               .WithQueryDataService(typeof(CSharpQuery), new CodeQueryDataService())
               .WithQueryDataService(typeof(XmlQuery), new XmlQueryDataService())
               .WithCodeLogicHandler(new RenameCodeLogicHandler())
               .WithXmlLogicHandler(new RenameXmlLogicHandler())
               .WithEnvDte(dte)
               .WithWorkspace(workspace);
        }
    }
}
