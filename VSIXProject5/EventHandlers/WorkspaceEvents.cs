using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using NLog;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Logging.MiniProfiler;
using IBatisSuperHelper.Storage;

namespace IBatisSuperHelper.Events
{
    public class WorkspaceEvents
    {
        private static bool _indexingBySolution;

        public static async Task BuildIndexerWithCSharpResults(Solution solution)
        {
            var solutionFiles = solution.Projects.SelectMany(x => x.Documents).ToList();
            await PackageStorage.AnalyzeAndStoreAsync(solutionFiles);
        }

        public static async Task BuildIndexerUsingProjectWithCSharpResults(Project project)
        {
            var solutionFiles = project.Documents.ToList();
            await PackageStorage.AnalyzeAndStoreAsync(solutionFiles);
        }

        private static async Task DocumentsAddedAction(IEnumerable<Document> addedDocuments)
        {
            CSharpIndexer csIndexer = new CSharpIndexer();
            if (addedDocuments == null)
                return;

            foreach (var document in addedDocuments)
            {
                await PackageStorage.AnalyzeAndStoreSingleAsync(document);
            }
        }
        private static async Task DocumentRemovedAction(IEnumerable<DocumentId> removedDocumentsIds)
        {
            foreach(var documentId in removedDocumentsIds)
            {
                PackageStorage.CodeQueries.RemoveStatmentsByDefinedObject(documentId);
            }
        }

        private static List<ProjectId> _projectsAlreadyAdded = new List<ProjectId>();

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
                            var solution = e.NewSolution.Projects.Any() ? e.NewSolution : workspace.CurrentSolution;
                            _projectsAlreadyAdded = new List<ProjectId>(solution.ProjectIds);
                            BuildIndexerWithCSharpResults(e.NewSolution.Projects.Any() ? e.NewSolution : workspace.CurrentSolution);
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
                            if (!_projectsAlreadyAdded.Any())
                            {
                                _projectsAlreadyAdded = new List<ProjectId>(e.NewSolution.ProjectIds);
                                Loggers.OutputWindowLogger.WriteLn($"Adding {string.Join(",", e.NewSolution.Projects.Select(x => x.Name))} to indexer");
                                BuildIndexerWithCSharpResults(e.NewSolution);
                            }
                            else
                            {
                                foreach (var project in e.NewSolution.Projects)
                                {
                                    if (!_projectsAlreadyAdded.Contains(project.Id))
                                    {
                                        _projectsAlreadyAdded.Add(project.Id);
                                        Loggers.OutputWindowLogger.WriteLn($"Adding {project.Name} to indexer");
                                        BuildIndexerUsingProjectWithCSharpResults(project);
                                    }
                                }
                            }
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
                    await DocumentsAddedAction(addedDocuments);
                    break;
                case WorkspaceChangeKind.DocumentRemoved:
                    var documentRemovedChanges = e.NewSolution.GetChanges(e.OldSolution);
                    var removedDocuments = documentRemovedChanges.GetProjectChanges()
                        .SelectMany(x => x.GetRemovedDocuments());
                    await DocumentRemovedAction(removedDocuments);
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
            profiler.Stop();
        }
    }
}
