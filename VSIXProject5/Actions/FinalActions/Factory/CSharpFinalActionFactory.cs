using Microsoft.VisualStudio.Shell;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Data;
using IBatisSuperHelper.Actions.FinalActions.SubActions.Logic;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Storage.Providers;
using IBatisSuperHelper.VSIntegration;

namespace IBatisSuperHelper.Actions.FinalActions.Factory
{
    public class CSharpFinalActionFactory : IFinalActionFactory
    {
        public FinalEventActionsExecutor GetFinalEventActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane)
        {
            var isHybridNamespaceHandlingEnabled = (bool)PackageStorage.RuntimeConfiguration.GetValue("HybridNamespaceEnabled");
            return FinalEventActionsExecutor
                .Create()
                .WithLogicHandler(new CodeLogicHandler(statusBar, toolWindowPane))
                .WithQueryDataService(new CodeQueryDataService<CSharpQuery>())
                .WithNamespaceHandlingLogicType(
                    isHybridNamespaceHandlingEnabled
                    ? NamespaceHandlingType.HYBRID_NAMESPACE
                    : NamespaceHandlingType.WITH_NAMESPACE);
        }
    }
}
