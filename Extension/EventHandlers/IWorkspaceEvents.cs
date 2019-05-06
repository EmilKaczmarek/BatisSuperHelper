using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IBatisSuperHelper.Events
{
    public interface IWorkspaceEvents
    {
        Task BuildIndexerUsingProjectWithCSharpResultsAsync(Project project);
        Task BuildIndexerWithCSharpResultsAsync(Solution solution);
        void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e);
    }
}