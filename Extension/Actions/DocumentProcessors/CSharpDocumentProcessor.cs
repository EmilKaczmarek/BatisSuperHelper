﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Actions.ActionValidators;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using Microsoft.CodeAnalysis.CSharp;

namespace BatisSuperHelper.Actions.DocumentProcessors
{
    public class CSharpDocumentProcessor : IDocumentProcessor
    {
        private readonly IActionValidator _validator;
        private readonly Document _documentContent;

        private IEnumerable<SyntaxNode> _lineNodes;
        private NodeHelpers _helperInstance;
        private Lazy<IEnumerable<SyntaxNode>> _documentNodes;
        private readonly int _selectedLineNumber;

        private CSharpDocumentProcessor()
        {

        }

        public CSharpDocumentProcessor(object documentContent, int selectedLineNumber) : this(documentContent, selectedLineNumber, new FunctionBasedActionValidator())
        {
            _validator = new FunctionBasedActionValidator()
               .WithFunctionList("jump", new List<Func<int, bool>>{
                   (selectedLine) => DoesNodeOrReturnNodeContainBatisNamespace()
               })
               .WithFunctionList("rename", new List<Func<int, bool>>
               {
                   (selectedLine)=>DoesNodeOrReturnNodeContainBatisNamespace() && IsAnyNodeLiteralStringExpression()
               });
        }

        public CSharpDocumentProcessor(object documentContent, int selectedLineNumber, IActionValidator validator)
        {
            _documentContent = (Document)documentContent;
            _selectedLineNumber = selectedLineNumber;
            _validator = validator;
        }

        public IDocumentProcessor Initialize()
        {
            if (_documentContent.TryGetSemanticModel(out SemanticModel semModel))
            {
                InitializeUsingSemanticModel(semModel);
                return this;
            }
            throw new InvalidOperationException($"Unable to get semantic model for: {_documentContent.Name}");
        }

        public async Task<IDocumentProcessor> InitializeAsync()
        {
            SemanticModel semModel = await _documentContent.GetSemanticModelAsync();
            InitializeUsingSemanticModel(semModel);

            return this;
        }

        private void InitializeUsingSemanticModel(SemanticModel semModel)
        {
            _helperInstance = new NodeHelpers(semModel);

            SyntaxTree synTree = semModel.SyntaxTree;
            var span = synTree.GetText().Lines[_selectedLineNumber].Span;
            var root = (CompilationUnitSyntax)synTree.GetRoot();

            _lineNodes = root.DescendantNodesAndSelf(span);
            _documentNodes = new Lazy<IEnumerable<SyntaxNode>>(() => root.DescendantNodesAndSelf());
        }

        private bool DoesNodeOrReturnNodeContainBatisNamespace()
        {
            var nodesAtLine = _lineNodes;
            var returnNode = _helperInstance.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
            if (returnNode != null)
            {
                nodesAtLine = returnNode.DescendantNodesAndSelf();
            }

            return _helperInstance.IsAnySyntaxNodeContainBatisNamespace(nodesAtLine);
        }

        private bool IsAnyNodeLiteralStringExpression()
        {
            var nodesAtLine = _lineNodes;
            var returnNode = _helperInstance.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
            if (returnNode != null)
            {
                nodesAtLine = returnNode.DescendantNodesAndSelf();
            }
            return _helperInstance.GetExpressionKindForBatisMethodArgument(nodesAtLine) == SyntaxKind.StringLiteralExpression;
        }

        public ExpressionResult GetQueryValueAtCurrentSelectedLine()
        {
            return this.GetQueryValueAtLine(_selectedLineNumber);
        }

        public ExpressionResult GetQueryValueAtLine(int lineNumber)
        {
            var returnNode = _helperInstance.GetFirstNodeOfReturnStatmentSyntaxType(_lineNodes);
            //Check if current document line is having 'return' keyword.
            //In this case we need to Descendant Node to find ArgumentList
            if (returnNode != null)
            {
                var expressionResolution = _helperInstance.GetQueryStringFromSyntaxNodes(_documentContent, _lineNodes, _documentNodes.Value);
                return expressionResolution;
            }
            //In case we don't have cursor around 'return', SyntaxNodes taken from line
            //should have needed ArgumentLineSyntax
            return _helperInstance.GetQueryStringFromSyntaxNodes(_documentContent, _lineNodes, _documentNodes.Value);
        }

        public IActionValidator GetValidator()
        {
            return _validator;
        }

        public bool TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue)
        {
            using (MiniProfiler.Current.Step(nameof(GetQueryValueAtCurrentSelectedLine)))
            {
                expressionResult = GetQueryValueAtCurrentSelectedLine();
            }
            
            if (expressionResult?.IsSolved == true)
            {
                queryValue = expressionResult.TextResult;
                return true;
            }
            if (expressionResult?.CanBeUsedAsQuery == true)
            {
                using (MiniProfiler.Current.Step(nameof(TryResolveExpression)))
                {
                    bool resolveSuccessful = TryResolveExpression(expressionResult, out string result);
                    queryValue = resolveSuccessful ? result : null;
                    return true;
                }
            }
            queryValue = null;

            return false;
        }

        private bool TryResolveExpression(ExpressionResult expressionResult, out string result)
        {
            result = null;
            switch (expressionResult.UnresolvedPart)
            {
                case UnresolvedPartType.None:
                    return false;
                case UnresolvedPartType.GenericClassName:
                    result = expressionResult.Resolve(_helperInstance.GetClassNameUsedAsGenericParameter(_lineNodes, _documentNodes.Value));
                    return !string.IsNullOrEmpty(result);
                default:
                    throw new InvalidOperationException("Unexpected Case");
            }
        }
    }
}
