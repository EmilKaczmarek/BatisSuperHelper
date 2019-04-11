using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration.ErrorList
{
    public class SinkManager : IDisposable
    {
        private readonly TableDataSource _errorList;
        private readonly ITableDataSink _sink;
        private readonly List<TableEntriesSnapshot> _snapshots = new List<TableEntriesSnapshot>();

        public SinkManager(TableDataSource errorList, ITableDataSink sink)
        {
            _errorList = errorList;
            _sink = sink;

            errorList.AddSinkManager(this);
        }

        public void Clear()
        {
            _sink.RemoveAllSnapshots();
        }

        public void Update(IEnumerable<TableEntriesSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
            {
                var existing = _snapshots.FirstOrDefault(s => s.FilePath == snapshot.FilePath);

                if (existing != null)
                {
                    _snapshots.Remove(existing);
                    _sink.ReplaceSnapshot(existing, snapshot);
                }
                else
                {
                    _sink.AddSnapshot(snapshot);
                }

                _snapshots.Add(snapshot);
            }
        }

        public void Remove(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                var existing = _snapshots.FirstOrDefault(s => s.FilePath == file);

                if (existing != null)
                {
                    _snapshots.Remove(existing);
                    _sink.RemoveSnapshot(existing);
                }
            }
        }

        public void Dispose()
        {
            _errorList.RemoveSinkManager(this);
        }
    }
}
