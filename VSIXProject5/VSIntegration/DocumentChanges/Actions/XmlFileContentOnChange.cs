using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NLog;
using StackExchange.Profiling;
using VSIXProject5.Constants;
using VSIXProject5.Indexers;
using VSIXProject5.Loggers;
using VSIXProject5.Logging.MiniProfiler;
using VSIXProject5.Parsers;
using VSIXProject5.Storage;

namespace VSIXProject5.VSIntegration.DocumentChanges.Actions
{
    public class XmlFileContentOnChange : IOnFileContentChange
    {
        public void HandleChange(IWpfTextView textView)
        {
            try
            {
                var profiler = MiniProfiler.StartNew(nameof(HandleChange));
                profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
                using (profiler.Step("HandleChangeXml"))
                {
                    var snapshot = textView.Caret.Position.BufferPosition.Snapshot;
                    ITextDocument textDoc;
                    textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDoc);
                    using (var stringReader = new StringReader(snapshot.GetText()))
                    {
                        var project = (Goto.Instance.package as GotoAsyncPackage).EnvDTE.Solution.FindProjectItem(textDoc.FilePath).ContainingProject.Name;
                        XmlParser parser = new XmlParser().WithStringReader(stringReader).WithFileInfo(textDoc.FilePath, project).Load();
                        if (parser.XmlNamespace == IBatisConstants.SqlMapNamespace)
                        {
                            var newStatments = parser.GetMapFileStatments();
                            PackageStorage.XmlQueries.UpdateStatmentForFileWihoutKey(newStatments);
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
