using Microsoft.VisualStudio.Shell;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Data;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.VSIntegration;

namespace IBatisSuperHelper.Actions.FinalActions.Factory
{
    public class XmlFinalActionFactory : IFinalActionFactory
    {
        public FinalEventActionsExecutor GetFinalEventActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            var isHybridNamespaceHandlingEnabled = (bool)PackageStorage.RuntimeConfiguration.GetValue("HybridNamespaceEnabled");
            return FinalEventActionsExecutor
                .Create()
                .WithLogicHandler(new XmlLogicHandler(statusBar, toolWindowPane))
                .WithQueryDataService(new XmlQueryDataService())
                .WithNamespaceHandlingLogicType(
                    isHybridNamespaceHandlingEnabled
                    ? NamespaceHandlingType.HYBRID_NAMESPACE
                    : NamespaceHandlingType.WITH_NAMESPACE);
        }
    }
}
