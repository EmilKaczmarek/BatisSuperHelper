using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.Validation.XmlValidators;
using IBatisSuperHelper.VSIntegration.ErrorList;
using IBatisSuperHelper.VSIntegration.Navigation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NLog;
using System;
using System.Collections.Generic;


namespace IBatisSuperHelper.VSIntegration.BatisFilesTextViewIntegration
{
    public class BatisMapErrorTagger : ITagger<IErrorTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private IBufferValidator _validator;
        private ITextBuffer _buffer;

        private ITextSnapshot _currentSnapshot;

        public BatisMapErrorTagger(IBufferValidator bufferValidator, ITextBuffer buffer)
        {
            _validator = bufferValidator;

            _buffer = buffer;
            _currentSnapshot = _buffer.CurrentSnapshot;
            _buffer.Changed += Buffer_Changed;

            _validator.ValidateAllSpans();
            _validator.AddToErrorList();
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
                    yield return new TagSpan<ErrorTag>(error.Span, new ErrorTag("Syntax error", error.Text));
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
