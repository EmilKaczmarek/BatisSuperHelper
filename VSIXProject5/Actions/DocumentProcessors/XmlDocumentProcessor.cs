using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Actions2.ActionValidators;
using VSIXProject5.HelpersAndExtensions;
using VSIXProject5.HelpersAndExtensions.Roslyn;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Parsers;
using static VSIXProject5.HelpersAndExtensions.XmlHelper;

namespace VSIXProject5.Actions2.DocumentProcessors
{
    public class XmlDocumentProcessor : IDocumentProcessor
    {
        private IActionValidator _validator { get; set; }
        private string _documentContent;
        private XmlParser _parser;
        private int _selectedLineNumber;

        public XmlDocumentProcessor(object documentContent, int selectedLineNumber) : this(documentContent, selectedLineNumber, new FunctionBasedActionValidator())
        {
            _validator = new FunctionBasedActionValidator()
                .WithFunctionList("jump", new List<Func<int, bool>>
                {
                    (selectionLineNum) => !XmlStringLine.IsIgnored(GetLineText(selectionLineNum + 1)),
                    (selectionLineNum) => _parser.XmlNamespace == @"http://ibatis.apache.org/mapping",
                    (selectionLineNum) => _parser.HasSelectedLineValidQuery(selectionLineNum + 1)
                })
                .WithFunctionList("rename", new List<Func<int, bool>>());
        }

        public XmlDocumentProcessor(object documentContent, int selectedLineNumber, IActionValidator validator)
        {
            _documentContent = (string)documentContent;
            _validator = validator;
            _selectedLineNumber = selectedLineNumber;         
        }

        public IDocumentProcessor Initialize()
        {
            using (var stringReader = new StringReader(_documentContent))
            {
                _parser = new XmlParser().WithStringReader(stringReader).Load();
            }

            return this;
        }

        public Task<IDocumentProcessor> InitializeAsync()
        {
            return Task.Run(() => this.Initialize());
        }

        private string GetLineText(int selectedLineNumber)
        {
            var splited = _documentContent.Split('\n');
            return splited[selectedLineNumber];
        }

        public IActionValidator GetValidator()
        {
            return _validator;
        }

        public ExpressionResult GetQueryValueAtLine(int lineNumber)
        {
            var elementLocation = _parser.GetStatmentElementsLineNumber().DetermineClosestInt(lineNumber + 1);
            var queryValue = _parser.GetQueryAtLineOrNull(elementLocation);
            return new ExpressionResult
            {
                IsSolved = queryValue != null,
                CanBeUsedAsQuery = queryValue != null,
                TextResult = queryValue
            };
        }

        public ExpressionResult GetQueryValueAtCurrentSelectedLine()
        {
            return GetQueryValueAtLine(_selectedLineNumber);
        }

        public bool TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue)
        {
            expressionResult = GetQueryValueAtCurrentSelectedLine();
            if (!expressionResult.IsSolved)
            {
                queryValue = null;
                return false;
            }

            queryValue = expressionResult.TextResult;
            return true;
            
        }
    }
}
