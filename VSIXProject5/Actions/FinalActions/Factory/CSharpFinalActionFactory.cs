using Microsoft.VisualStudio.Shell;
using VSIXProject5.Actions.FinalActions.SubActions.Data;
using VSIXProject5.Actions.FinalActions.SubActions.Logic;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Storage;
using VSIXProject5.Storage.Providers;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions.FinalActions.Factory
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
