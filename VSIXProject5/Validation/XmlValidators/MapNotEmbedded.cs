using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace IBatisSuperHelper.Validation.XmlValidators
{
    public class MapNotEmbedded : IXmlValidator
    {
        private bool _isRunning = false;
        public bool IsRunning => _isRunning;

        private readonly List<BatisError> _errors = new List<BatisError>();
        public List<BatisError> Errors => _errors;

        private IClassifier _classifier;
        private SnapshotSpan _span;
        private ITextDocument _document;
        private ITextBuffer _buffer;

        public MapNotEmbedded(IClassifier classifier, SnapshotSpan span, ITextDocument document, ITextBuffer buffer)
        {
            _classifier = classifier;
            _span = span;
            _document = document;
            _buffer = buffer;
         
            ValidateAllSpans();
        }

        public void OnChange(SnapshotSpan newSpans)
        {
            _errors.Clear();
            _span = newSpans;
            ValidateAllSpans();
        }

        public void ValidateAllSpans()
        {
            _isRunning = true;
            var classificationSpans = _classifier.GetClassificationSpans(_span);
            var isBatisMapFileCSpan = classificationSpans.FirstOrDefault(e => e.ClassificationType.Classification == "XML Attribute Value" && e.Span.GetText().Equals(IBatisConstants.SqlMapNamespace));
            if (isBatisMapFileCSpan != null)
            {
                var projectItem = GotoAsyncPackage.EnvDTE.Solution.FindProjectItem(_document.FilePath);
                if (projectItem != null)
                {
                    var buildAction = projectItem.Properties.Item("BuildAction").Value;
                    if ((int)buildAction != 3)
                    {
                        AddError(isBatisMapFileCSpan.Span.Start.GetContainingLine(), isBatisMapFileCSpan.Span, $"Map file build action should be embedded.");
                    }
                }
            }
            TableDataSource.Instance.AddErrors(_errors);
            _isRunning = false;
        }

        private void AddError(ITextSnapshotLine line, SnapshotSpan span, string message)
        {
            var error = new BatisError
            {
                Span = span,
                Text = message,
                Line = line.LineNumber,
                Column = span.Start.Position - line.Start.Position,
                Category = TaskCategory.Misc,
                Document = _document.FilePath,
            };

            if (!_errors.Any(e => e.Line == error.Line &&
                                e.Column == error.Column &&
                                e.Text == error.Text &&
                                e.Document == error.Document))
            {
                _errors.Add(error);
            }
        }
    }
}
