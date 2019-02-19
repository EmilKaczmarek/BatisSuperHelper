using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NLog;
using StackExchange.Profiling;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Logging.MiniProfiler;
using IBatisSuperHelper.Storage;

namespace IBatisSuperHelper.VSIntegration.DocumentChanges.Actions
{
    public class CSharpFileContentOnChange : IOnFileContentChange
    {
        public void HandleChange(IWpfTextView textView)
        {
            try
            {
                var profiler = MiniProfiler.StartNew(nameof(HandleChange));
                profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
                using (profiler.Step("HandleChangeCode"))
                {
                    SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
                    Microsoft.CodeAnalysis.Document roslynDocument = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
                    if (roslynDocument == null)
                    {
                        OutputWindowLogger.WriteLn($"Unable to get c# file to process. Name {caretPosition.Snapshot.ContentType.DisplayName}");
                        return;
                    }

                    PackageStorage.AnalyzeAndUpdateSingle(roslynDocument);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "CSharpFileContentOnChange.HandleChange");
                OutputWindowLogger.WriteLn($"Exception occured during handling csharp file change: {ex.Message}");
            }
        }
    }
}
