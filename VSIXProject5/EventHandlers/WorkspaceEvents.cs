using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Indexer.Instance.Build(codeIndexerResult);
        }
        private static async void DocumentsAddedAction(IEnumerable<Document> addedDocuments)
        {
            CSharpIndexer csIndexer = new CSharpIndexer();

            foreach (var document in addedDocuments)
            {
                var documentIndexerResult = await csIndexer.BuildFromDocumentAsync(document);
                Indexer.Instance.Build(documentIndexerResult);
            }
        }
        private static async void DocumentRemovedAction(IEnumerable<DocumentId> removedDocumentsIds)
        {
            foreach(var documentId in removedDocumentsIds)
            {
                Indexer.Instance.RemoveCodeStatmentsForDocumentId(documentId);
            }
        }
        public static void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var workspace = sender as VisualStudioWorkspace;
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
                    var documentAddedChanges = e.NewSolution.GetChanges(e.OldSolution);
                    var addedDocuments = documentAddedChanges.GetProjectChanges()
                        .SelectMany(x => x.GetAddedDocuments())
                        .Select(x => workspace.CurrentSolution.GetDocument(x));
                    DocumentsAddedAction(addedDocuments);
                    break;
                case WorkspaceChangeKind.DocumentRemoved:
                    var documentRemovedChanges = e.NewSolution.GetChanges(e.OldSolution);
                    var removedDocuments = documentRemovedChanges.GetProjectChanges()
                        .SelectMany(x => x.GetRemovedDocuments());
                    DocumentRemovedAction(removedDocuments);
                    break;
                case WorkspaceChangeKind.DocumentReloaded:
                    break;
                case WorkspaceChangeKind.DocumentChanged:
                    var changes3 = e.NewSolution.GetChanges(e.OldSolution);
                    var t12 = documentAddedChanges.GetAddedProjects();
                    var t22 = documentAddedChanges.GetProjectChanges();
                    var t32 = documentAddedChanges.GetRemovedProjects();
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
