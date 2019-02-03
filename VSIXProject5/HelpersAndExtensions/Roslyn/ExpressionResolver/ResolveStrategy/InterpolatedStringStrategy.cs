using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Profiling;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class InterpolatedStringStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            using (MiniProfiler.Current.Step(nameof(InterpolatedStringStrategy)))
            {
                var interpolatedNode = expressionSyntax as InterpolatedStringExpressionSyntax;
                List<ExpressionResult> expressionResolverResults = new List<ExpressionResult>();
                foreach (var content in interpolatedNode.Contents)
                {
                    if (content is InterpolationSyntax)
                    {
                        var expression = (content as InterpolationSyntax).Expression;
                        expressionResolverResults.Add(expressionResolverInstance.GetStringValueOfExpression(document, expression, nodes, semanticModel));
                    }
                    if (content is InterpolatedStringTextSyntax)
                    {
                        expressionResolverResults.Add(new ExpressionResult
                        {
                            IsSolved = true,
                            CanBeUsedAsQuery = false,
                            ExpressionText = interpolatedNode.ToString(),
                            TextResult = (content as InterpolatedStringTextSyntax).TextToken.Text,
                        });
                    }
                }
                if (expressionResolverResults.Any(e => !e.IsSolved && e.CanBeUsedAsQuery))
                {
                    return new ExpressionResult
                    {
                        CanBeUsedAsQuery = !(expressionResolverResults.Count(e => !e.IsSolved && e.CanBeUsedAsQuery) > 1),//Atm, only allow for 1 variable
                        ExpressionText = interpolatedNode.ToString(),
                        IsSolved = false,
                        TextResult = string.Join("", expressionResolverResults.Select(e => e.TextResult)),
                        UnresolvableReason = expressionResolverResults.First(e => !e.IsSolved).TextResult,
                        UnresolvedPart = expressionResolverResults.First(e => !e.IsSolved).UnresolvedPart,
                        UnresolvedValue = expressionResolverResults.First(e => !e.IsSolved).UnresolvedValue,
                        UnresolvedFormat = string.Join("", expressionResolverResults.Select(e => e.IsSolved ? e.TextResult : "{0}")),
                    };
                }
                return new ExpressionResult
                {
                    CanBeUsedAsQuery = false,
                    ExpressionText = string.Join("", expressionResolverResults.Select(e => e.TextResult)),
                    IsSolved = true,
                    TextResult = string.Join("", expressionResolverResults.Select(e => e.TextResult)),
                };
            }
        }
    }
}
