using System;
using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NLog;
using StackExchange.Profiling;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Logging.MiniProfiler;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Storage;
using BatisSuperHelper.Constants.BatisConstants;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;

namespace BatisSuperHelper.VSIntegration.DocumentChanges.Actions
{
    public class XmlFileContentOnChange : IOnFileContentChange
    {
        public async System.Threading.Tasks.Task HandleChangeAsync(IWpfTextView textView)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var profiler = MiniProfiler.StartNew(nameof(HandleChangeAsync));
                profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
                using (profiler.Step("HandleChangeXml"))
                {
                    var snapshot = textView.Caret.Position.BufferPosition.Snapshot;
                    ITextDocument textDoc;
                    textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDoc);
                    using (var stringReader = new StringReader(snapshot.GetText()))
                    {
                        var project = GotoAsyncPackage.EnvDTE.Solution.FindProjectItem(textDoc.FilePath)?.ContainingProject?.Name;
                        XmlParser baseParser = new XmlParser(stringReader).Load();
                        
                        if (baseParser.BatisXmlFileType == BatisXmlFileTypeEnum.SqlMap)
                        {
                            BatisXmlMapParser parser = new BatisXmlMapParser(baseParser).WithFileInfo(textDoc.FilePath, project);
                            var newStatments = parser.GetMapFileStatments();
                            GotoAsyncPackage.Storage.XmlQueries.UpdateStatmentForFileWihoutKey(newStatments);
                        }
                        if (baseParser.BatisXmlFileType == BatisXmlFileTypeEnum.SqlMapConfig)
                        {
                            BatisXmlConfigParser parser = new BatisXmlConfigParser(baseParser).WithFileInfo(textDoc.FilePath, project);
                            GotoAsyncPackage.Storage.SqlMapConfigProvider.UpdateOrAddConfig(parser.Result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "XmlFileContentOnChange.HandleChange");
                OutputWindowLogger.WriteLn($"Exception occured during handling xml file change: {ex.Message}");
            }
        }
    }
}
