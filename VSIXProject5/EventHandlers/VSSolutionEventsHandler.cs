using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;
using IBatisSuperHelper.Constants;
using Microsoft.VisualStudio.Shell;
using static IBatisSuperHelper.Constants.EventConstats.VS;

namespace IBatisSuperHelper.Events
{
    public class VSSolutionEventsHandler
    {
        internal class SolutionEventsHandler : IVsSolutionLoadEvents, IVsSolutionEvents
        {
            private Action<SolutionLoad> _handler;
            public SolutionEventsHandler(Action<SolutionLoad> handlerAction)
            {
                if (handlerAction == null)
                    throw new Exception("Event handler action is null");
                _handler = handlerAction;
            }
            public int OnBeforeOpenSolution(string pszSolutionFilename)
            {
                return VSConstants.S_OK;
            }

            public int OnBeforeBackgroundSolutionLoadBegins()
            {
                return VSConstants.S_OK;
            }

            public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
            {
                pfShouldDelayLoadToNextIdle = false;
                return VSConstants.S_OK;
            }

            public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
            {
                return VSConstants.S_OK;
            }

            public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
            {
                return VSConstants.S_OK;
            }

            public int OnAfterBackgroundSolutionLoadComplete()
            {
                 _handler(SolutionLoad.SolutionLoadComplete);
                //Load Xml files from indexer
                return VSConstants.S_OK;
            }

            public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
            {
                return VSConstants.S_OK;
            }

            public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
            {
                return VSConstants.S_OK;
            }

            public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
            {
                return VSConstants.S_OK;
            }

            public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
            {
                return VSConstants.S_OK;
            }

            public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
            {
                return VSConstants.S_OK;
            }

            public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
            {
                return VSConstants.S_OK;
            }

            public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
            {
                return VSConstants.S_OK;
            }

            public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
            {
                //Remove everything from indexer instance
                _handler(SolutionLoad.SolutionOnClose);
                return 1;
            }

            public int OnBeforeCloseSolution(object pUnkReserved)
            {
                return VSConstants.S_OK;
            }

            public int OnAfterCloseSolution(object pUnkReserved)
            {
                return VSConstants.S_OK;
            }
        }
    }
}
