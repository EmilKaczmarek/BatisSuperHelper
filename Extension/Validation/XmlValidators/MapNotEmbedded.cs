using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Constants.BatisConstants;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace IBatisSuperHelper.Validation.XmlValidators
{
    public class MapNotEmbedded : IXmlValidator, IBufferValidator, IBuildDocumentValidator
    {
        public bool IsRunning => _isRunning;
        private bool _isRunning = false;
        
        public List<BatisError> Errors => _errors;
        private readonly List<BatisError> _errors = new List<BatisError>();

        private readonly IClassifier _classifier;
        private SnapshotSpan _span;
        private readonly ITextDocument _document;

        private readonly string _filePath;
        private readonly BatisXmlMapParser _xmlParser;

        public MapNotEmbedded(IClassifier classifier, SnapshotSpan span, ITextDocument document, ITextBuffer buffer)
        {
            _classifier = classifier;
            _span = span;
            _document = document;
         
            ValidateAllSpans();
        }

        public MapNotEmbedded(string filePath)
        {
            _filePath = filePath;
            _xmlParser = new BatisXmlMapParser().WithFileInfo(_filePath, "").Load();
        }

        public void OnChange(SnapshotSpan newSpan)
        {
            _errors.Clear();
            _span = newSpan;
            ValidateAllSpans();
        }

        public void ValidateBuildDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _isRunning = true;
            if (IsDocumentSupportedForValidation())
            {
                var projectItem = GotoAsyncPackage.EnvDTE.Solution.FindProjectItem(_filePath);
                if (projectItem != null)
                {
                    var buildAction = projectItem.Properties.Item("BuildAction").Value;
                    if ((int)buildAction != 3)
                    {
                        AddError($"Map file build action should be embedded.");
                    }
                }
            }
            _isRunning = false;
        }

        public void ValidateAllSpans()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _isRunning = true;
            var classificationSpans = _classifier.GetClassificationSpans(_span);
            var isBatisMapFileCSpan = classificationSpans.FirstOrDefault(e => e.ClassificationType.Classification == "XML Attribute Value" && e.Span.GetText().Equals(XmlMapConstants.SqlMapNamespace));
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
                ErrorCode = "IB003",
            };

            if (!_errors.Any(e => e.Line == error.Line &&
                                e.Column == error.Column &&
                                e.Text == error.Text &&
                                e.Document == error.Document))
            {
                _errors.Add(error);
            }
        }

        private void AddError( string message)
        {
            var error = new BatisError
            {
                Text = message,
                Line = 0,
                Column = 0,
                Category = TaskCategory.Misc,
                Document = _filePath,
                ErrorCode = "IB003",
            };

            if (!_errors.Any(e => e.Line == error.Line &&
                                e.Column == error.Column &&
                                e.Text == error.Text &&
                                e.Document == error.Document))
            {
                _errors.Add(error);
            }
        }

        public bool IsDocumentSupportedForValidation()
        {
            return _xmlParser.XmlNamespace == XmlMapConstants.SqlMapNamespace;
        }

        public void AddToErrorList()
        {
            TableDataSource.Instance.AddErrors(_errors);
        }
    }
}
