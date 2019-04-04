using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using NLog;

namespace IBatisSuperHelper.Validation.XmlValidators
{
    public class XmlValidatorsAggregator : IXmlValidator, IBuildDocumentValidator, IBufferValidator
    {
        public static XmlValidatorsAggregator Create => new XmlValidatorsAggregator();
        private readonly List<IXmlValidator> _validators = new List<IXmlValidator>();

        private IClassifier _classifier;
        private SnapshotSpan _span;
        private ITextDocument _document;
        private IWpfTextView _view;
        private ITextBuffer _buffer;

        private string _filePath;

        private XmlValidatorsAggregator()
        {
            _validators.Clear();
        }

        public XmlValidatorsAggregator AllValidatorsForBuffer(IClassifier classifier, SnapshotSpan span, ITextDocument document, IWpfTextView view, ITextBuffer buffer)
        {
            _classifier = classifier;
            _span = span;
            _document = document;
            _view = view;
            _buffer = buffer;

            _validators.Add(new QueryUnused(_classifier, _span, _document, _buffer));
            _validators.Add(new MapNotEmbedded(_classifier, _span, _document, _buffer));

            return this;
        }

        public XmlValidatorsAggregator AllValidatorsForBuild(string filePath)
        {
            _filePath = filePath;

            _validators.Add(new QueryUnused(_filePath));
            _validators.Add(new MapNotEmbedded(_filePath));

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

        public void ValidateBuildDocument()
        {
            try
            {
                foreach (var validator in _validators)
                {
                    (validator as IBuildDocumentValidator).ValidateBuildDocument();
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "XmlValidatorsAggregator.ValidateBuildDocument");
                OutputWindowLogger.WriteLn($"Exception occured during XmlValidatorsAggregator.ValidateBuildDocument: { ex.Message}");
            }
        }

        public void ValidateAllSpans()
        {
            try
            {
                foreach (var validator in _validators)
                {
                    (validator as IBufferValidator).ValidateAllSpans();
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "XmlValidatorsAggregator.ValidateAllSpans");
                OutputWindowLogger.WriteLn($"Exception occured during XmlValidatorsAggregator.ValidateAllSpans: { ex.Message}");
            }
        }

        public List<BatisError> GetErrorsFromValidators()
        {
            return _validators.SelectMany(e => e.Errors).ToList();
        }

        public void AddToErrorList()
        {
            //Because of bug with errors not showing in list, 
            //aggregator should add all results from validators,
            //without calling "AddToErrorList" in every validator.
            TableDataSource.Instance.AddErrors(Errors);
        }
    }
}
