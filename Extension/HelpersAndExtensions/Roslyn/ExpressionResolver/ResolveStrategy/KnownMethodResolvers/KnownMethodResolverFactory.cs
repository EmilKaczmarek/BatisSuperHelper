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
    public class KnownMethodResolverFactory
    {
        private readonly InvocationExpressionSyntax _analyzedInvocation;
        private readonly ExpressionSyntax _nextInvocationExpression;

        private readonly ExpressionResolver _expressionResolverInstance;
        private readonly Document _document;
        private readonly IEnumerable<SyntaxNode> _nodes;
        private readonly SemanticModel _semanticModel;

        public KnownMethodResolverFactory(InvocationExpressionSyntax analyzedInvocationExpression, ExpressionSyntax nextInvocationExpression, ExpressionResolver expressionResolverInstance, Document document, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _nextInvocationExpression = nextInvocationExpression;
            _analyzedInvocation = analyzedInvocationExpression;
            _expressionResolverInstance = expressionResolverInstance;
            _document = document;
            _nodes = nodes;
            _semanticModel = semanticModel;
        }

        public IKnownMethodResolver GetResolver()
        {
            var nextNextInvocationExpression = _nextInvocationExpression as MemberAccessExpressionSyntax;
            var predefinedTypeExpression = nextNextInvocationExpression.Expression as PredefinedTypeSyntax;
            var indentifierName = nextNextInvocationExpression.Name as IdentifierNameSyntax;
            if (predefinedTypeExpression?.Keyword.ValueText == "string" && indentifierName?.Identifier.ValueText == "Format")
            {
                return new StringFormatResolver(_analyzedInvocation, _expressionResolverInstance, _document, _nodes, _semanticModel);
            }
            var type = _semanticModel.GetTypeInfo(nextNextInvocationExpression.Expression);

            if (type.Type.Name == "String" && indentifierName?.Identifier.ValueText == "Replace")
            {
                return new StringReplaceResolver(_analyzedInvocation, nextNextInvocationExpression, _expressionResolverInstance, _document, _nodes, _semanticModel);
            }
            return new UnsupportedMethodResolver();
        }
        
       
    }
}
