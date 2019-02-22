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

        [Import]
        IBufferTagAggregatorFactoryService _s = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (!buffer.Properties.TryGetProperty(typeof(ErrorListProvider), out ErrorListProvider errorProvider))
                return null;

            if (!buffer.Properties.TryGetProperty(typeof(IWpfTextView), out IWpfTextView view))
                return null;

            TextDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document);
            return new BatisMapErrorTagger(view, _classifierAggregatorService, errorProvider, document) as ITagger<T>;

        }
    }
}
