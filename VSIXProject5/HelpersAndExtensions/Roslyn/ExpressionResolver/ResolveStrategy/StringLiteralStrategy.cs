using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Profiling;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class StringLiteralStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            using (MiniProfiler.Current.Step(nameof(StringLiteralStrategy)))
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
}
