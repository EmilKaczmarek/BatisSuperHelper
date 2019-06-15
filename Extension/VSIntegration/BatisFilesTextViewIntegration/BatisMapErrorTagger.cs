using BatisSuperHelper.Constants;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Storage;
using BatisSuperHelper.Storage.Event;
using BatisSuperHelper.Validation.XmlValidators;
using BatisSuperHelper.VSIntegration.ErrorList;
using BatisSuperHelper.VSIntegration.Navigation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NLog;
using System;
using System.Collections.Generic;


namespace BatisSuperHelper.VSIntegration.BatisFilesTextViewIntegration
{
    public class BatisMapErrorTagger : ITagger<IErrorTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private readonly IBufferValidator _validator;
        private readonly ITextBuffer _buffer;
       
        private ITextSnapshot _currentSnapshot;

        public BatisMapErrorTagger(IBufferValidator bufferValidator, ITextView textView, ITextBuffer buffer)
        {
            _validator = bufferValidator;

            _buffer = buffer;
            _currentSnapshot = _buffer.CurrentSnapshot;
            _buffer.Changed += Buffer_Changed;

            textView.LayoutChanged += TextView_LayoutChanged;
            textView.Caret.PositionChanged += Caret_PositionChanged;

            _validator.ValidateAllSpans();
            _validator.AddToErrorList();
            StorageEvents.OnStoreChange += Storage_OnStoreChange;
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            TagsChanged?.Invoke(sender, new SnapshotSpanEventArgs(new SnapshotSpan(e.NewPosition.BufferPosition.Snapshot, 0, e.NewPosition.BufferPosition.Snapshot.Length)));
        }

        private void TextView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            TagsChanged?.Invoke(sender, new SnapshotSpanEventArgs(new SnapshotSpan(e.NewSnapshot, 0, e.NewSnapshot.Length)));
        }

        private void Storage_OnStoreChange(object sender, StoreChangeEventArgs e)
        {
            if (e.ChangedFileType.IsSet(ChangedFileTypeFlag.CSharp))
            {
                if (TagsChanged != null)
                {
                    var span = new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);
                    _validator.OnChange(span);
                    _validator.AddToErrorList();
                    TagsChanged.Invoke(sender, new SnapshotSpanEventArgs(span));
                }
            }
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans[0].Snapshot != _currentSnapshot)
                yield break;

            if (_validator.IsRunning)
                yield break;

            foreach (var error in _validator.Errors)
            {
                if (error.Span.Snapshot != spans[0].Snapshot)
                {
                    //This case/bug should be resolved, if this is not the case, log this one, 
                    //and ignore this validation error.
                    LogManager.GetLogger("error").Error($"Snapshots are not same in tagger.");
                    continue;
                }
                if (spans.IntersectsWith(error.Span))
                {
                    //yield return new TagSpan<IErrorTag>(error.Span, new ErrorTag(error.TaggerErrorType, error.Text));
                    yield return new TagSpan<IErrorTag>(error.Span, new ErrorTag(PredefinedErrorTypeNames.CompilerError, error.Text));
                }
            }
        }

        private void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            _currentSnapshot = _buffer.CurrentSnapshot;
            _validator.OnChange(new SnapshotSpan(e.After, 0, e.After.Length));
            _validator.AddToErrorList();
        }
    }
}
