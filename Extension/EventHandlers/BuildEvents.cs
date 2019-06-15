using EnvDTE;
using BatisSuperHelper.CoreAutomation.ProjectItems;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Validation.XmlValidators;
using BatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using NLog;
using System;

namespace BatisSuperHelper.EventHandlers
{
    public class BuildEventsActions
    {
        public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var projectItemHelper = new ProjectItemRetreiver(GotoAsyncPackage.EnvDTE);
                var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects();

                foreach (var xmlFile in DocumentHelper.GetXmlFiles(projectItems))
                {
                    var validator = XmlValidatorsAggregator.Create.AllValidatorsForBuild(xmlFile.FilePath);
                    validator.ValidateBuildDocument();
                    validator.AddToErrorList();
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "BuildEvents.OnBuildBegin");
                OutputWindowLogger.WriteLn($"Exception occured during BuildEvents.OnBuildBegin: { ex.Message}");
            }

        }
    }
}
