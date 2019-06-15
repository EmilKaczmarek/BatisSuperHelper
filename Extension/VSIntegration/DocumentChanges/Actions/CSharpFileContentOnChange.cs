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
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Logging.MiniProfiler;
using BatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;

namespace BatisSuperHelper.VSIntegration.DocumentChanges.Actions
{
    public class CSharpFileContentOnChange : IOnFileContentChange
    {
        public async System.Threading.Tasks.Task HandleChangeAsync(IWpfTextView textView)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var profiler = MiniProfiler.StartNew(nameof(HandleChangeAsync));
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

                    await GotoAsyncPackage.Storage.AnalyzeAndUpdateSingleAsync(roslynDocument);
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
