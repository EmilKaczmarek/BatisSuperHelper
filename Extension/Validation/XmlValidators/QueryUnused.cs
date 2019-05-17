﻿using EnvDTE;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.Constants.BatisConstants;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Parsers.Models.SqlMap;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Validation.XmlValidators
{
    public class QueryUnused : IXmlValidator, IBufferValidator, IBuildDocumentValidator
    {
        private readonly IClassifier _classifier;
        private SnapshotSpan _span;
        private readonly ITextDocument _document;
        private BatisXmlMapParser _xmlParser;

        private readonly string _filePath;

        private readonly List<BatisError> _errors = new List<BatisError>();
        public List<BatisError> Errors => _errors;

        private bool _isRunning = false;
        public bool IsRunning => _isRunning;

        public QueryUnused(IClassifier classifier, SnapshotSpan span, ITextDocument document)
        {
            _classifier = classifier;
            _span = span;
            _document = document;
            _xmlParser = new BatisXmlMapParser().WithStringReader(new StringReader(span.Snapshot.GetText())).Load();
            ValidateAllSpans();
        }

        public QueryUnused(string filePath)
        {
            _filePath = filePath;
            _xmlParser = new BatisXmlMapParser().WithFileInfo(_filePath, "").Load();
        }

        public void OnChange(SnapshotSpan newSpan)
        {
            ClearErrors();
            _span = newSpan;
            _xmlParser = new BatisXmlMapParser().WithStringReader(new StringReader(newSpan.Snapshot.GetText())).Load();
            ValidateAllSpans();
        }

        public void ClearErrors()
        {
            _errors.Clear();
        }

        public bool IsDocumentSupportedForValidation()
        {
            return _xmlParser.XmlNamespace == XmlMapConstants.XmlNamespace;
        }

        public void ValidateAllSpans()
        {
            _isRunning = true;
            ClearErrors();
            var classificationSpans = _classifier.GetClassificationSpans(_span);
            foreach (var cSpan in classificationSpans)
            {
                if (cSpan.ClassificationType.Classification == "XML Name" &&
                    XmlMapConstants.StatementNames.Contains(cSpan.Span.GetText())
                    && cSpan.Span.Start.GetContainingLine().Extent.GetText().Contains("id"))
                {
                    var line = cSpan.Span.Start.GetContainingLine();
                    if (_xmlParser.HasSelectedLineValidQuery(line.LineNumber + 1))
                    {
                        var query = _xmlParser.GetQueryAtLineOrNull(line.LineNumber + 1, GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces);
                        var queryUsages = GotoAsyncPackage.Storage.CodeQueries.GetKeysByQueryId(query, GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces);
                        if (!queryUsages.Any())
                        {
                            var statmentIdCSpan = classificationSpans.FirstOrDefault(e =>
                                e.Span.GetText().Trim() == query &&
                                e.Span.IntersectsWith(line.Extent));

                            AddError(line, statmentIdCSpan?.Span ?? cSpan.Span, $"Query {query} is unused.");
                        }
                    }
                }
            }
            _isRunning = false;
        }

        public void ValidateBuildDocument()
        {
            _isRunning = true;
            ClearErrors();

            var parserResults = _xmlParser.GetMapFileStatmentsWithIdAttributeColumnInfo();
            foreach (var result in parserResults)
            {
                var queryUsages = GotoAsyncPackage.Storage.CodeQueries.GetKeysByQueryId(result.FullyQualifiedQuery, GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces);
                if (!queryUsages.Any())
                {
                    AddError(result, $"Query {result.QueryId} is unused.");
                }
            }
            _isRunning = false;
        }

        private void AddError(Statement query, string message)
        {
            var error = new BatisError
            {
                Text = message,
                Line = query.XmlLine.HasValue ? query.XmlLine.Value - 1 : 0,
                Column = query.XmlLineColumn.HasValue ? query.XmlLineColumn.Value + 2 : 0,
                Category = TaskCategory.Misc,
                Document = _filePath,
                ErrorCode = "IB002",
            };

            if (!_errors.Any(e => e.Line == error.Line &&
                                e.Column == error.Column &&
                                e.Text == error.Text &&
                                e.Document == error.Document))
            {
                _errors.Add(error);
            }
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
                ErrorCode = "IB002",
            };

            if (!_errors.Any(e=>e.Line == error.Line && 
                                e.Column == error.Column && 
                                e.Text == error.Text &&
                                e.Document == error.Document))
            {
                _errors.Add(error);
            }
        }

        public void AddToErrorList()
        {
            TableDataSource.Instance.AddErrors(_errors);
        }
    }
}
