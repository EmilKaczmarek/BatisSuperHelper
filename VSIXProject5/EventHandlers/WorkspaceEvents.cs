using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Indexers;

namespace VSIXProject5.Events
{
    public class WorkspaceEvents
    {
        
        private static async void BuildIndexerWithCSharpResults(Solution solution)
        {
            var solutionFiles = solution.Projects.SelectMany(x => x.Documents).ToList();
            CSharpIndexer csIndexer = new CSharpIndexer();
            var codeIndexerResult = await csIndexer.BuildIndexerAsync(solutionFiles);
            Indexer.Build(codeIndexerResult);
        }
        public static void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
                    BuildIndexerWithCSharpResults(e.NewSolution);
                    break;
                case WorkspaceChangeKind.SolutionChanged:
                    break;
                case WorkspaceChangeKind.SolutionRemoved:
                    break;
                case WorkspaceChangeKind.SolutionCleared:
                    break;
                case WorkspaceChangeKind.SolutionReloaded:
                    break;
                case WorkspaceChangeKind.ProjectAdded:
                    break;
                case WorkspaceChangeKind.ProjectRemoved:
                    break;
                case WorkspaceChangeKind.ProjectChanged:
                    break;
                case WorkspaceChangeKind.ProjectReloaded:
                    break;
                case WorkspaceChangeKind.DocumentAdded:
                    break;
                case WorkspaceChangeKind.DocumentRemoved:
                    break;
                case WorkspaceChangeKind.DocumentReloaded:
                    break;
                case WorkspaceChangeKind.DocumentChanged:
                    break;
                case WorkspaceChangeKind.AdditionalDocumentAdded:
                    break;
                case WorkspaceChangeKind.AdditionalDocumentRemoved:
                    break;
                case WorkspaceChangeKind.AdditionalDocumentReloaded:
                    break;
                case WorkspaceChangeKind.AdditionalDocumentChanged:
                    break;
                default:
                    throw new Exception("Unexpected Case");
            }
        }
    }
}
