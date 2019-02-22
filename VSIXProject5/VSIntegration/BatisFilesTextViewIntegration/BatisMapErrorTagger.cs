using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Storage;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IBatisSuperHelper.VSIntegration.BatisFilesTextViewIntegration
{
    public class BatisMapErrorTagger : ITagger<IErrorTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private IClassifier _classifier;
        private ErrorListProvider _errorlist;
        private ITextDocument _document;
        private IWpfTextView _view;

        public BatisMapErrorTagger(IWpfTextView view, IClassifierAggregatorService classifier, ErrorListProvider errorlist, ITextDocument document)
        {
            _view = view;
            _classifier = classifier.GetClassifier(view.TextBuffer);
            _errorlist = errorlist;
            _document = document;
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var span = spans[0];
            var line = span.Start.GetContainingLine();
            var classificationSpans = _classifier.GetClassificationSpans(line.Extent);

            RemoveErrorIfExists(line);

            var xmlNames = classificationSpans.FirstOrDefault(e => e.ClassificationType.Classification == "XML Name" &&
                IBatisConstants.StatementNames.Contains(e.Span.GetText()));
            if (xmlNames != null)
            {
                var parser = new XmlParser().WithStringReader(new StringReader(span.Snapshot.GetText())).Load();
                var mapNamespace = parser.MapNamespace;
                if (parser.HasSelectedLineValidQuery(line.LineNumber + 1))
                {
                    var query = parser.GetQueryAtLineOrNull(line.LineNumber + 1, true);
                    var queryUsages = PackageStorage.CodeQueries.GetKeysByQueryId(query, Storage.Providers.NamespaceHandlingType.HYBRID_NAMESPACE);
                    if (!queryUsages.Any())
                    {
                        var cSpan = classificationSpans.FirstOrDefault(e => e.Span.GetText() == query);
                        if (cSpan != null)
                        {
                            yield return CreateError(line, cSpan.Span, $"Query {query} is unused.");
                        }
                    }
                }
            } 
        }

        private bool DoesAlreadyExistsInErrorList(ITextSnapshotLine line, string message)
        {
            foreach (ErrorTask existing in _errorlist.Tasks)
            {
                if (existing.Line == line.LineNumber && existing.Text.Equals(message))
                    return true;
            }
            return false;
        }

        private void RemoveErrorIfExists(ITextSnapshotLine line)
        {
            foreach (ErrorTask existing in _errorlist.Tasks)
            {
                if (existing.Line == line.LineNumber)
                {
                    _errorlist.Tasks.Remove(existing);
                    break;
                }
            }
        }

        private TagSpan<ErrorTag> CreateError(ITextSnapshotLine line, SnapshotSpan span, string message)
        {
            if (!DoesAlreadyExistsInErrorList(line, message))
            {
                _errorlist.Tasks.Add(CreateErrorTask(line, span, message));
            }

            return new TagSpan<ErrorTag>(span, new ErrorTag("Syntax error", message));
        }

        private ErrorTask CreateErrorTask(ITextSnapshotLine line, SnapshotSpan span, string text)
        {
            ErrorTask errorTask = new ErrorTask
            {
                Text = text,
                Line = line.LineNumber,
                Column = span.Start.Position - line.Start.Position,
                Priority = TaskPriority.Normal,
                Category = TaskCategory.Misc,
                ErrorCategory = TaskErrorCategory.Warning,
                Document = _document.FilePath,
                HelpKeyword = "TestHelp",
            };

            errorTask.Navigate += (sender, e) =>
            {
                ErrorTask task = (ErrorTask)sender;
                _errorlist.Navigate(task, new Guid("{00000000-0000-0000-0000-000000000000}"));

                var errLine = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(task.Line);
                var point = new SnapshotPoint(errLine.Snapshot, errLine.Start.Position + task.Column);
                _view.Caret.MoveTo(point);
            };

            return errorTask;
        }
    }
}
