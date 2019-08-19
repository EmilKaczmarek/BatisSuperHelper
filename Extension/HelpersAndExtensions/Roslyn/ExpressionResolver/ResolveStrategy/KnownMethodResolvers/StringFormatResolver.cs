using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy.KnownMethodResolvers
{
    public class StringFormatResolver : IKnownMethodResolver
    {
        private readonly InvocationExpressionSyntax _expressionSyntax;
        private readonly ExpressionResolver _expressionResolverInstance;
        private readonly Document _document;
        private readonly IEnumerable<SyntaxNode> _nodes;
        private readonly SemanticModel _semanticModel;
        public StringFormatResolver(InvocationExpressionSyntax expressionSyntax, ExpressionResolver expressionResolverInstance, Document document, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _expressionSyntax = expressionSyntax;
            _expressionResolverInstance = expressionResolverInstance;
            _document = document;
            _nodes = nodes;
            _semanticModel = semanticModel;
        }

        public ExpressionResult Resolve()
        {
            string format = "";
            if (_expressionSyntax.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax)
            {
                format = (_expressionSyntax.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.ValueText;
            }
            List<string> parameters = new List<string>();
            List<ExpressionResult> expressionResolverResults = new List<ExpressionResult>();
            foreach (var parameter in _expressionSyntax.ArgumentList.Arguments.Skip(1))
            {
                expressionResolverResults.Add(_expressionResolverInstance.GetStringValueOfExpression(_document, parameter.Expression, _nodes, _semanticModel));
            }

            if (expressionResolverResults.Any(e => !e.IsSolved && e.CanBeUsedAsQuery))
            {
                return new ExpressionResult
                {
                    CanBeUsedAsQuery = !(expressionResolverResults.Count(e => !e.IsSolved && e.CanBeUsedAsQuery) > 1),//Atm, only allow for 1 variable
                    ExpressionText = _expressionSyntax.ToString(),
                    IsSolved = false,
                    TextResult = string.Format(format, expressionResolverResults.Select(e => e.TextResult).ToArray()),
                    UnresolvableReason = expressionResolverResults.First(e => !e.IsSolved).TextResult,
                    UnresolvedPart = expressionResolverResults.First(e => !e.IsSolved).UnresolvedPart,
                    UnresolvedValue = expressionResolverResults.First(e => !e.IsSolved).UnresolvedValue,
                    UnresolvedFormat = string.Join("", expressionResolverResults.Select(e => e.IsSolved ? e.TextResult : "{0}")),
                };
            }
            return new ExpressionResult
            {
                CanBeUsedAsQuery = false,
                ExpressionText = _expressionSyntax.ToString(),
                IsSolved = true,
                TextResult = string.Format(format, expressionResolverResults.Select(e => e.TextResult).ToArray()),
            };
        }
    }
}
