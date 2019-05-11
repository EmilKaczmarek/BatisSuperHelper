using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Actions.ActionValidators;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Parsers;
using static IBatisSuperHelper.HelpersAndExtensions.XmlHelper;
using IBatisSuperHelper.Constants.BatisConstants;

namespace IBatisSuperHelper.Actions.DocumentProcessors
{
    public class XmlDocumentProcessor : IDocumentProcessor
    {
        private IActionValidator _validator { get; set; }
        private readonly string _documentContent;
        private BatisXmlMapParser _parser;
        private readonly int _selectedLineNumber;

        public XmlDocumentProcessor(object documentContent, int selectedLineNumber) : this(documentContent, selectedLineNumber, new FunctionBasedActionValidator())
        {
            _validator = new FunctionBasedActionValidator()
                .WithFunctionList("jump", new List<Func<int, bool>>
                {
                    (selectionLineNum) => !XmlStringLine.IsIgnored(GetLineText(selectionLineNum + 1)),
                    (selectionLineNum) => _parser.XmlNamespace == XmlMapConstants.XmlNamespace,
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
                _parser = new BatisXmlMapParser().WithStringReader(stringReader).Load();
            }

            return this;
        }

        public async Task<IDocumentProcessor> InitializeAsync()
        {
            return await Task.Run(() => this.Initialize());
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
            var queryValue = _parser.GetQueryAtLineOrNull(elementLocation, GotoAsyncPackage.Storage.SqlMapConfigProvider.GetCurrentSettings().UseStatementNamespaces);
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
            using (MiniProfiler.Current.Step(nameof(TryResolveQueryValueAtCurrentSelectedLine)))
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
}
