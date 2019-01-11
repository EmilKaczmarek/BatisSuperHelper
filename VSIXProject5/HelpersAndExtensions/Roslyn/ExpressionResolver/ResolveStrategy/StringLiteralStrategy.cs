using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class StringLiteralStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            var token = (expressionSyntax as LiteralExpressionSyntax).Token;
            var tokenValue = token.Value;
            return new ExpressionResult
            {
                CanBeUsedAsQuery = false,
                ExpressionText = expressionSyntax.ToString(),
                IsSolved = true,
                TextResult = token.ValueText,
            };
        }
    }
}
