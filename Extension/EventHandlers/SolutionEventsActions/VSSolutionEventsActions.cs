using EnvDTE80;
using BatisSuperHelper.Constants;
using BatisSuperHelper.CoreAutomation.ProjectItems;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Workflow;
using BatisSuperHelper.Indexers.Xml;
using BatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.EventHandlers.SolutionEventsActions
{
    public class VSSolutionEventsActions : IVSSolutionEventsActions
    {
        private readonly IndexingWorkflow _indexingWorkflow;

        public VSSolutionEventsActions(IndexingWorkflow indexingWorkflow)
        {
            _indexingWorkflow = indexingWorkflow;
        }

        public void OnSolutionLoadComplete()
        {
            _indexingWorkflow.GetAndSetProjectItems();
            _indexingWorkflow.ExecuteIndexing();
        }

        public void SolutionOnClose()
        {
            GotoAsyncPackage.Storage.Clear();
        }
    }
}
