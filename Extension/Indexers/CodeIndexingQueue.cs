using IBatisSuperHelper.Logging.MiniProfiler;
using IBatisSuperHelper.Storage;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using NLog;
using StackExchange.Profiling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Indexers
{
    public class ProjectIndexingQueue : IDisposable
    {
        private ConcurrentDictionary<ProjectId, Lazy<Project>> _projects = new ConcurrentDictionary<ProjectId, Lazy<Project>>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10);
        private ConcurrentQueue<System.Threading.Tasks.Task> _tasks = new ConcurrentQueue<System.Threading.Tasks.Task>();

        private event EventHandler OnProjectAdded;

        public bool IsProcessing => _semaphore.CurrentCount == 1;

        public ProjectIndexingQueue()
        {
            OnProjectAdded += (e, args) => ThreadHelper.JoinableTaskFactory.RunAsync(async () => await HandleProjectAddedAsync());
        }

        private async System.Threading.Tasks.Task HandleProjectAddedAsync()
        {
            var profiler = MiniProfiler.StartNew(nameof(HandleProjectAddedAsync));
            profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
            using (profiler.Step("while(_task.Any())"))
            {
                while (_tasks.Any())
                {
                    await _semaphore.WaitAsync();
                    if (_tasks.TryDequeue(out System.Threading.Tasks.Task task))
                    {
                        await task.ContinueWith((t) => _semaphore.Release(), TaskScheduler.Default);
                    }
                    else
                    {
                        _semaphore.Release();
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task EnqueueAsync(Project project)
        {
            if (!_projects.ContainsKey(project.Id))
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    _projects.TryAdd(project.Id, new Lazy<Project>(() => project));
                    foreach (var document in project.Documents)
                    {
                        _tasks.Enqueue(CreateIndexingTask(project.Id, document.Id));
                    }
                    OnProjectAdded?.Invoke(this, EventArgs.Empty);
                });
            }
            
        }

        public async System.Threading.Tasks.Task EnqueueMultipleAsync(IEnumerable<Project> projects)
        {
            foreach (var project in projects)
            {
                await EnqueueAsync(project);
            }
        }

        private System.Threading.Tasks.Task CreateIndexingTask(ProjectId projectId, DocumentId documentId)
        {
            var document = _projects[projectId].Value.GetDocument(documentId);
            return PackageStorage.AnalyzeAndStoreSingleAsync(document);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _semaphore.Dispose();
                }
                _projects = null;
                _tasks = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
