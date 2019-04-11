using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration.ErrorList
{
    public class TableDataSource : ITableDataSource
    {
        public static TableDataSource Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TableDataSource();
                }
                return _instance;
            }
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static TableDataSource _instance;
        private readonly List<SinkManager> _sinkManagers = new List<SinkManager>();
        private static Dictionary<string, TableEntriesSnapshot> _snapshots = new Dictionary<string, TableEntriesSnapshot>();


        [Import]
        private ITableManagerProvider TableManagerProvider { get; set; } = null;

        private TableDataSource()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var compositionService = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(compositionService);

            compositionService.DefaultCompositionService.SatisfyImportsOnce(this);

            var manager = TableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            manager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander,
                                    StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
                                    StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
                                    StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.ErrorCategory,
                                    StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName, 
                                    StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);
        
        }

        public void AddErrors(IEnumerable<BatisError> errors)
        {
            if (errors == null || !errors.Any())
                return;

            foreach (var error in errors.GroupBy(t => t.Document))
            {
                var snapshot = new TableEntriesSnapshot(error.Key, error);
                _snapshots[error.Key] = snapshot;
            }

            UpdateAllSinkManagers();
        }

        public void CleanErrors(IEnumerable<string> files)
        {
            RemoveFromSnapshots(files);

            RemoveShapshotManagers(files);

            UpdateAllSinkManagers();
        }

        private void RemoveFromSnapshots(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (_snapshots.ContainsKey(file))
                {
                    _snapshots[file].Dispose();
                    _snapshots.Remove(file);
                }
            }
        }

        private void RemoveShapshotManagers(IEnumerable<string> files)
        {
            _semaphore.Wait();
            foreach (var manager in _sinkManagers)
            {
                manager.Remove(files);
            }
            _semaphore.Release();
        }

        private void RemovAllSnapshotManagers()
        {
            _semaphore.Wait();
            foreach (var manager in _sinkManagers)
            {
                manager.Clear();
            }
            _semaphore.Release();
        }

        public void CleanAllErrors()
        {
            foreach (var file in _snapshots.Keys)
            {
                var snapshot = _snapshots[file];
                if (snapshot != null)
                {
                    snapshot.Dispose();
                }
            }
            _snapshots.Clear();

            RemovAllSnapshotManagers();
        }

        public bool HasAnyErrors()
        {
            return _snapshots.Any();
        }

        public bool HasErrors(string file)
        {
            return _snapshots.ContainsKey(file);
        }

        public void AddSinkManager(SinkManager sinkManager)
        {
            _semaphore.Wait();
            _sinkManagers.Add(sinkManager);
            _semaphore.Release();
        }

        public void RemoveSinkManager(SinkManager sinkManager)
        {
            _semaphore.Wait();
            _sinkManagers.Remove(sinkManager);
            _semaphore.Release();
        }

        public void UpdateAllSinkManagers()
        {
            _semaphore.Wait();
            foreach (var manager in _sinkManagers)
            {
                manager.Update(_snapshots.Values);
            }
            _semaphore.Release();
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            return new SinkManager(this, sink);
        }

        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        public string Identifier => "BatisExtension";

        public string DisplayName => "BatisExtension";
    }
}
