using NLog;
using StackExchange.Profiling;
using StackExchange.Profiling.Helpers;
using StackExchange.Profiling.Internal;
using StackExchange.Profiling.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Logging.MiniProfiler
{
    public class NLogStorage : IAsyncStorage
    {
        private Logger _logger;
        private LogLevel _logLevel;

        public NLogStorage() : this(LogManager.GetCurrentClassLogger(), LogLevel.Trace)
        {

        }

        public NLogStorage(Logger logger) : this(logger, LogLevel.Trace)
        {

        }

        public NLogStorage(Logger logger, LogLevel logLevel)
        {
            //Idea of throwing early seems better than debugging wtf happend in another library...
            _logger = logger ?? throw new ArgumentNullException($"{nameof(Logger)} parameter is null");
            _logLevel = logLevel ?? throw new ArgumentNullException($"{nameof(LogLevel)} parameter is null");
        }

        public List<Guid> GetUnviewedIds(string user)
        {
            return new List<Guid>();
        }

        public Task<List<Guid>> GetUnviewedIdsAsync(string user)
        {
            return Task.FromResult(GetUnviewedIds(user));
        }

        public IEnumerable<Guid> List(int maxResults, DateTime? start = null, DateTime? finish = null, ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            return Enumerable.Empty<Guid>();
        }

        public Task<IEnumerable<Guid>> ListAsync(int maxResults, DateTime? start = null, DateTime? finish = null, ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            return Task.FromResult(List(maxResults));
        }

        public StackExchange.Profiling.MiniProfiler Load(Guid id)
        {
            return StackExchange.Profiling.MiniProfiler.Current;
        }

        public Task<StackExchange.Profiling.MiniProfiler> LoadAsync(Guid id)
        {
            return Task.FromResult(Load(id));
        }

        public void Save(StackExchange.Profiling.MiniProfiler profiler)
        {
            if (profiler == null)
                return;
            _logger.Log(_logLevel, profiler.ToJson());
        }

        public Task SaveAsync(StackExchange.Profiling.MiniProfiler profiler)
        {
            return Task.FromResult<object>(null);
        }

        public void SetUnviewed(string user, Guid id)
        {
        }

        public Task SetUnviewedAsync(string user, Guid id)
        {
            return Task.FromResult<object>(null);
        }

        public void SetViewed(string user, Guid id)
        {
        }

        public Task SetViewedAsync(string user, Guid id)
        {
            return Task.FromResult<object>(null);
        }
    }
}
