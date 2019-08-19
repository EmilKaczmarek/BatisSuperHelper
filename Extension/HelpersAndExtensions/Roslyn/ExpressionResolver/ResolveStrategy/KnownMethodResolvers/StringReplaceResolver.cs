using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy.KnownMethodResolvers
{
    public class StringReplaceResolver : IKnownMethodResolver
    {
        private readonly InvocationExpressionSyntax _analyzedInvocation;
        private readonly MemberAccessExpressionSyntax _nextExpressionSyntax;
        private readonly ExpressionResolver _expressionResolverInstance;
        private readonly Document _document;
        private readonly IEnumerable<SyntaxNode> _nodes;
        private readonly SemanticModel _semanticModel;

        public StringReplaceResolver(InvocationExpressionSyntax analyzedInvocation, MemberAccessExpressionSyntax nextExpressionSyntax, ExpressionResolver expressionResolverInstance, Document document, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _analyzedInvocation = analyzedInvocation;
            _nextExpressionSyntax = nextExpressionSyntax; 
            _expressionResolverInstance = expressionResolverInstance;
            _document = document;
            _nodes = nodes;
            _semanticModel = semanticModel;
        }

        public ExpressionResult Resolve()
        {
            if (_analyzedInvocation.ArgumentList.Arguments.Any())
            {
                var replacingInVariableResolution = _expressionResolverInstance.GetStringValueOfExpression(_document, _nextExpressionSyntax.Expression, _nodes, _semanticModel);
                var stringToReplaceResolution = _expressionResolverInstance.GetStringValueOfExpression(_document, _analyzedInvocation.ArgumentList.Arguments[0].Expression, _nodes, _semanticModel);
                var stringToReplaceWithResolution = _expressionResolverInstance.GetStringValueOfExpression(_document, _analyzedInvocation.ArgumentList.Arguments[1].Expression, _nodes, _semanticModel);
                if (replacingInVariableResolution.IsSolved && stringToReplaceResolution.IsSolved && stringToReplaceWithResolution.IsSolved)
                {
                    return new ExpressionResult
                    {
                        CanBeUsedAsQuery = false,
                        ExpressionText = _analyzedInvocation.ToString(),
                        IsSolved = true,
                        TextResult = replacingInVariableResolution.TextResult.Replace(stringToReplaceResolution.TextResult, stringToReplaceWithResolution.TextResult)
                    };
                }
                return new ExpressionResult
                {
                    CanBeUsedAsQuery = false,
                    ExpressionText = _analyzedInvocation.ToString(),
                    IsSolved = false,
                    UnresolvableReason = "Unable to resolve all variables needed.",
                };
            }
            return new ExpressionResult
            {
                CanBeUsedAsQuery = false,
                ExpressionText = _analyzedInvocation.ToString(),
                IsSolved = false,
                UnresolvableReason = "No argument provided to method.",
            };
        }
    }
}
