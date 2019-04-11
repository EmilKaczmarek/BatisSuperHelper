using EnvDTE;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Validation.XmlValidators;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.EventHandlers
{
    public class BuildEvents
    {
        public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var projectItemHelper = new ProjectItemHelper();
                var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects(GotoAsyncPackage.EnvDTE.Solution.Projects);

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
