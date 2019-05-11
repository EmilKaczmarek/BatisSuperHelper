using IBatisSuperHelper.EventHandlers;
using IBatisSuperHelper.EventHandlers.SolutionEventsActions;
using IBatisSuperHelper.Indexers.Workflow;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using System;
using System.Diagnostics;
using static IBatisSuperHelper.Constants.EventConstats.VS;

namespace IBatisSuperHelper.Events
{
    public class VSSolutionEventsHandler
    {
        internal class SolutionEventsHandler : IVsSolutionLoadEvents, IVsSolutionEvents
        {
            private readonly IVSSolutionEventsActions _actions;

            public SolutionEventsHandler(IVSSolutionEventsActions actions)
            {
                _actions = actions;
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
                Debug.WriteLine("OnAfterBackgroundSolutionLoadComplete()");
                try
                {
                    //Load Xml files from indexer
                    _actions.OnSolutionLoadComplete();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("error").Error(ex, "SolutionEventsHandler.OnAfterBackgroundSolutionLoadComplete");
                    OutputWindowLogger.WriteLn($"Exception occured during SolutionEventsHandler.OnAfterBackgroundSolutionLoadComplete: { ex.Message}");
                }
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
                try
                {
                    //Remove everything from indexer instance
                    _actions.SolutionOnClose();
                    //Remove Errors
                    TableDataSource.Instance.CleanAllErrors();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("error").Error(ex, "SolutionEventsHandler.OnQueryCloseSolution");
                    OutputWindowLogger.WriteLn($"Exception occured during SolutionEventsHandler.OnQueryCloseSolution: { ex.Message}");
                }
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
