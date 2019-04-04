using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Logging.MiniProfiler;

namespace IBatisSuperHelper.Logging
{
    public static class NLogConfigurationService
    {
        public static readonly string AssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static void ConfigureNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var profilerFile = new FileTarget("profilerFile")
            {
                FileName = $"{AssemblyLocation}/logs/profiler.json",
                ArchiveFileName = $"{AssemblyLocation}/logs/profiler.{"{####}"}.json",
                Layout = "${message}",
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                ArchiveOldFileOnStartup = true,
            };

            var errorFile = new FileTarget("errorLogFile")
            {
                FileName = $"{AssemblyLocation}/logs/log.txt",
                Layout = $"{"${longdate} ${message} ${exception:format=StackTrace:maxInnerExceptionLevel=5}"}",
            };

            Debug.WriteLine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            config.AddTarget("profiler", profilerFile);
            config.AddTarget("error", errorFile);

            config.AddRule(LogLevel.Trace, LogLevel.Trace, profilerFile);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, errorFile);
            LogManager.Configuration = config;
        }

        public static void ConfigureMiniProfilerWithDefaultLogger()
        {
            Logger logger = LogManager.GetLogger("profiler");
            StackExchange.Profiling.MiniProfiler.DefaultOptions.Storage = new NLogStorage(logger, LogLevel.Trace);
        }
    }
}
