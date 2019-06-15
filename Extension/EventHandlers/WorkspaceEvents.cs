using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using NLog;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Logging.MiniProfiler;
using BatisSuperHelper.Storage;
using Microsoft.VisualStudio.Threading;
using BatisSuperHelper.Indexers.Code;
using Microsoft.VisualStudio.Shell;

namespace BatisSuperHelper.Events
{
    public class WorkspaceEvents
    {
        private ProjectIndexingQueue _indexingQueue;

        public WorkspaceEvents() : this(new ProjectIndexingQueue())
        {

        }

        public WorkspaceEvents(ProjectIndexingQueue indexingQueue)
        {
            _indexingQueue = indexingQueue;
        }

        private async System.Threading.Tasks.Task DocumentsAddedActionAsync(IEnumerable<Document> addedDocuments)
        {
            if (addedDocuments == null)
                return;

            foreach (var document in addedDocuments)
            {
                await GotoAsyncPackage.Storage.AnalyzeAndStoreSingleAsync(document);
            }
        }
        private async System.Threading.Tasks.Task DocumentRemovedActionAsync(IEnumerable<DocumentId> removedDocumentsIds)
        {
            foreach(var documentId in removedDocumentsIds)
            {
                await System.Threading.Tasks.Task.Run(() => GotoAsyncPackage.Storage.CodeQueries.RemoveStatmentsByDefinedObject(documentId));
            }
        }

        public async System.Threading.Tasks.Task WorkspaceChangedAsync(object sender, WorkspaceChangeEventArgs e)
        {
            var profiler = MiniProfiler.StartNew(nameof(WorkspaceChangedAsync));
            profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
            var workspace = sender as VisualStudioWorkspace;
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
                    try
                    {
                        using (profiler.Step(WorkspaceChangeKind.SolutionAdded.ToString()))
                        {
                            await _indexingQueue.EnqueueMultipleAsync(workspace.CurrentSolution.Projects);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("error").Error(ex, "WorkspaceChangeKind.SolutionAdded");
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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
                            await _indexingQueue.EnqueueMultipleAsync(workspace.CurrentSolution.Projects);
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
            await profiler.StopAsync();
        }
    }
}
