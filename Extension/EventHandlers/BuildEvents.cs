using EnvDTE;
using IBatisSuperHelper.CoreAutomation.ProjectItems;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Validation.XmlValidators;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using NLog;
using System;

namespace IBatisSuperHelper.EventHandlers
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
