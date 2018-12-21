using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Loggers;
using VSIXProject5.Storage;

namespace VSIXProject5.VSIntegration.DocumentChanges.Actions
{
    public class CSharpFileContentOnChange : IOnFileContentChange
    {
        public void HandleChange(IWpfTextView textView)
        {
            SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
            Microsoft.CodeAnalysis.Document roslynDocument = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (roslynDocument == null)
            {
                OutputWindowLogger.WriteLn($"Unable to get c# file to process. Name {caretPosition.Snapshot.ContentType.DisplayName}");
                return;
            }
            var csIndexer = new CSharpIndexer().BuildFromDocumentAsync(roslynDocument).Result;
            PackageStorage.CodeQueries.UpdateStatmentForFileWihoutKey(new List<List<CSharpQuery>> { csIndexer.Queries });
        }
    }
}
