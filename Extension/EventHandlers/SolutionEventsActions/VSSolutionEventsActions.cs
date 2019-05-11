using EnvDTE80;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.CoreAutomation.ProjectItems;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.Indexers.Workflow;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.EventHandlers.SolutionEventsActions
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
            GotoAsyncPackage.Storage.CodeQueries.Clear();
            GotoAsyncPackage.Storage.XmlQueries.Clear();
        }
    }
}
