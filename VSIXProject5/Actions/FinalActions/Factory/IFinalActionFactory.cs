using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Actions;
using IBatisSuperHelper.VSIntegration;

namespace IBatisSuperHelper.Actions.FinalActions.Factory
{
    public interface IFinalActionFactory
    {
        FinalEventActionsExecutor GetFinalEventActionsExecutor(StatusBarIntegration statusBar, ToolWindowPane toolWindowPane);
    }
}
