using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Actions;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions.FinalActions.Factory
{
    public interface IFinalActionFactory
    {
        FinalEventActionsExecutor GetFinalEventActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane);
    }
}
