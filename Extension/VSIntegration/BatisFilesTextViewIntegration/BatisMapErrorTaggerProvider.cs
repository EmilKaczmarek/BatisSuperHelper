using IBatisSuperHelper.Validation.XmlValidators;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration.BatisFilesTextViewIntegration
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("XML")]
    [TagType(typeof(ErrorTag))]
    public class BatisMapErrorTaggerProvider : ITaggerProvider
    {
        [Import]
        private ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        IClassifierAggregatorService _classifierAggregatorService = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (!buffer.Properties.TryGetProperty(typeof(ErrorListProvider), out ErrorListProvider errorProvider))
                return null;

            if (!buffer.Properties.TryGetProperty(typeof(IWpfTextView), out IWpfTextView _view))
                return null;
    
            TextDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document);
            var classifier = _classifierAggregatorService.GetClassifier(_view.TextBuffer);
            var span = new SnapshotSpan(_view.TextBuffer.CurrentSnapshot, 0, _view.TextBuffer.CurrentSnapshot.Length);

            var validator = XmlValidatorsAggregator
                .Create
                .AllValidatorsForBuffer(classifier, span, document, _view, buffer);
            return new BatisMapErrorTagger(validator, buffer) as ITagger<T>;
        }
    }
}
