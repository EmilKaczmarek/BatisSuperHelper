using EnvDTE;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Validation.XmlValidators;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
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
            var projectItemHelper = new ProjectItemHelper();
            var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects(GotoAsyncPackage.EnvDTE.Solution.Projects);

            foreach (var xmlFile in DocumentHelper.GetXmlFiles(projectItems))
            {
                var validator = XmlValidatorsAggregator.Create.AllValidatorsForBuild(xmlFile.FilePath);
                validator.ValidateBuildDocument();
                validator.AddToErrorList();
            }

        }
    }
}
