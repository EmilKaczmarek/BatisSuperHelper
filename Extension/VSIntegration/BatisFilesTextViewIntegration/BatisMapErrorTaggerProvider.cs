using BatisSuperHelper.Validation.XmlValidators;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.VSIntegration.BatisFilesTextViewIntegration
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("XML")]
    [TagType(typeof(ErrorTag))]
    public class BatisMapErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        private ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        IClassifierAggregatorService _classifierAggregatorService = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            TextDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document);
            var classifier = _classifierAggregatorService.GetClassifier(textView.TextBuffer);
            var span = new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, 0, textView.TextBuffer.CurrentSnapshot.Length);

            var validator = XmlValidatorsAggregator
                .Create
                .AllValidatorsForBuffer(classifier, span, document, buffer);
           

            return new BatisMapErrorTagger(validator, textView, buffer) as ITagger<T>;
        }


    }
}
