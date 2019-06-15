using EnvDTE;
using EnvDTE80;
using BatisSuperHelper.CoreAutomation.ProjectItems;
using BatisSuperHelper.Indexers.Workflow;
using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.Indexers.Workflow.Strategies.Config;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Tests
{
    public class ConfigWorkflowTests
    {
        [Fact]
        public void ShouldCallDependeciesMethods()
        {
            //var projectItemRetreiverMock = new Mock<IProjectItemRetreiver>();
            //projectItemRetreiverMock.Setup(e => e.GetProjectItemsFromSolutionProjects()).Returns(Enumerable.Empty<ProjectItem>);
            //var configStrategyMock = new Mock<IConfigStrategy>();
            //var wfOptions = new IndexingWorkflowOptions();

            //IndexingWorkflow wf = new IndexingWorkflow(wfOptions, );
            //wf.ExecuteIndexing();

            //configStrategyMock.Verify(e => e.Process());
            //TODO: Replace ThreadHelper with single instance and uncomment bottom line
            //projectItemRetreiverMock.Verify(e => e.GetProjectItemsFromSolutionProjects());
        }
    }
}
