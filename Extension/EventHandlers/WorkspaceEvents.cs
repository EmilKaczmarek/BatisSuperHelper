using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using NLog;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Logging.MiniProfiler;
using IBatisSuperHelper.Storage;
using Microsoft.VisualStudio.Threading;
using IBatisSuperHelper.Indexers.Code;

namespace IBatisSuperHelper.Events
{
    public static class WorkspaceEvents
    {
        private static ProjectIndexingQueue _indexingQueue = new ProjectIndexingQueue();

        public static async Task BuildIndexerWithCSharpResultsAsync(Solution solution)
        {
            var solutionFiles = solution.Projects.SelectMany(x => x.Documents).ToList();
            await PackageStorage.AnalyzeAndStoreAsync(solutionFiles);
        }

        public static async Task BuildIndexerUsingProjectWithCSharpResultsAsync(Project project)
        {
            var solutionFiles = project.Documents.ToList();
            await PackageStorage.AnalyzeAndStoreAsync(solutionFiles);
        }

        private static async Task DocumentsAddedActionAsync(IEnumerable<Document> addedDocuments)
        {
            if (addedDocuments == null)
                return;

            foreach (var document in addedDocuments)
            {
                await PackageStorage.AnalyzeAndStoreSingleAsync(document);
            }
        }
        private static async Task DocumentRemovedActionAsync(IEnumerable<DocumentId> removedDocumentsIds)
        {
            foreach(var documentId in removedDocumentsIds)
            {
                await Task.Run(() => PackageStorage.CodeQueries.RemoveStatmentsByDefinedObject(documentId));
            }
        }

        public static async void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var profiler = MiniProfiler.StartNew(nameof(WorkspaceChanged));
            profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
            var workspace = sender as VisualStudioWorkspace;
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
                    try
                    {
                        using (profiler.Step(WorkspaceChangeKind.SolutionAdded.ToString()))
                        {
                            await _indexingQueue.EnqueueMultipleAsync(e.NewSolution.Projects.Any() ? e.NewSolution.Projects : workspace.CurrentSolution.Projects);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("error").Error(ex, "WorkspaceChangeKind.SolutionAdded");
                        OutputWindowLogger.WriteLn($"Exception occured during adding solution: {ex.Message}");
                    }
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
                    try
                    {
                        using (profiler.Step(WorkspaceChangeKind.SolutionAdded.ToString()))
                        {
                            await _indexingQueue.EnqueueMultipleAsync(e.NewSolution.Projects);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("error").Error(ex, "WorkspaceChangeKind.ProjectAdded");
                        OutputWindowLogger.WriteLn($"Exception occured during adding projects: {ex.Message}");
                    }
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
                    await DocumentsAddedActionAsync(addedDocuments);
                    break;
                case WorkspaceChangeKind.DocumentRemoved:
                    var documentRemovedChanges = e.NewSolution.GetChanges(e.OldSolution);
                    var removedDocuments = documentRemovedChanges.GetProjectChanges()
                        .SelectMany(x => x.GetRemovedDocuments());
                    await DocumentRemovedActionAsync(removedDocuments);
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
                    break;
            }
            profiler.Stop();
        }
    }
}
