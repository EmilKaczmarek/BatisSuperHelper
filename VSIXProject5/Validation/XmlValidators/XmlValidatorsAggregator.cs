using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace IBatisSuperHelper.Validation.XmlValidators
{
    public enum ValidatorType
    {
        /// <summary>
        /// Use for specific buffer.
        /// </summary>
        Buffer,
        /// <summary>
        /// Use for on build actions.
        /// </summary>
        Build,
    }

    public class XmlValidatorsAggregator : IXmlValidator
    {
        public static XmlValidatorsAggregator Create => new XmlValidatorsAggregator();
        private readonly List<IXmlValidator> _validators = new List<IXmlValidator>();
        private ValidatorType _type;

        private IEnumerable<IXmlValidator> _bufferValidators => _validators;
        private IClassifier _classifier;
        private SnapshotSpan _span;
        private ITextDocument _document;
        private IWpfTextView _view;
        private ITextBuffer _buffer;

        //Used for debugging only.
        private Guid _guid = Guid.NewGuid();

        private XmlValidatorsAggregator()
        {
            _validators.Clear();
        }

        public XmlValidatorsAggregator ForBuffer(IClassifier classifier, SnapshotSpan span, ITextDocument document, IWpfTextView view, ITextBuffer buffer)
        {
            _type = ValidatorType.Buffer;
            _classifier = classifier;
            _span = span;
            _document = document;
            _view = view;
            _buffer = buffer;
            return this;
        }

        public XmlValidatorsAggregator All()
        {
            _validators.Add(new QueryUnused(_classifier, _span, _document, _buffer));
            _validators.Add(new MapNotEmbedded(_classifier, _span, _document, _buffer));
            return this;
        }

        public XmlValidatorsAggregator With(IXmlValidator xmlValidator)
        {
            _validators.Add(xmlValidator);
            return this;
        }

        public bool IsRunning => _validators.Any(e => e.IsRunning);

        public List<BatisError> Errors => GetErrorsFromValidators();

        public void OnChange(SnapshotSpan span)
        {
            foreach (var validator in _validators)
            {
                validator.OnChange(span);
            }
        }

        public List<BatisError> GetErrorsFromValidators()
        {
            Debug.WriteLine($"Requested erros from Validator: {_guid}, {_document.FilePath}");
            return _validators.SelectMany(e => e.Errors).ToList();
        }
    }
}
