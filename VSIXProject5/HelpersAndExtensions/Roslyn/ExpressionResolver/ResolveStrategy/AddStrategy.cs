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
    public class AddStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            using (MiniProfiler.Current.Step(nameof(AddStrategy)))
            {
                var binaryExpressionNode = expressionSyntax as BinaryExpressionSyntax;
                var leftExpression = binaryExpressionNode.Left;
                var rightExpression = binaryExpressionNode.Right;
                var leftExpressionValue = expressionResolverInstance.GetStringValueOfExpression(document, leftExpression, nodes, semanticModel);
                var rightExpressionValue = expressionResolverInstance.GetStringValueOfExpression(document, rightExpression, nodes, semanticModel);
                if (leftExpressionValue.IsSolved && rightExpressionValue.IsSolved)
                {
                    return new ExpressionResult
                    {
                        IsSolved = true,
                        CanBeUsedAsQuery = false,
                        ExpressionText = expressionSyntax.ToString(),
                        TextResult = leftExpressionValue.TextResult + rightExpressionValue.TextResult,
                    };
                }
                var result = new ExpressionResult
                {
                    IsSolved = false,
                    ExpressionText = expressionSyntax.ToString(),
                };

                if (!leftExpressionValue.IsSolved)
                {
                    result.TextResult += leftExpressionValue.TextResult;
                    result.CanBeUsedAsQuery = leftExpressionValue.CanBeUsedAsQuery;
                    result.UnresolvableReason = leftExpressionValue.UnresolvableReason;
                    result.UnresolvedPart = leftExpressionValue.UnresolvedPart;
                    result.UnresolvedValue = leftExpressionValue.UnresolvedValue;
                    result.UnresolvedFormat = "{0}" + rightExpressionValue.TextResult;
                }

                if (!rightExpressionValue.IsSolved)
                {
                    result.TextResult += rightExpressionValue.TextResult;
                    result.CanBeUsedAsQuery = rightExpressionValue.CanBeUsedAsQuery;
                    result.UnresolvableReason = rightExpressionValue.UnresolvableReason;
                    result.UnresolvedPart = rightExpressionValue.UnresolvedPart;
                    result.UnresolvedValue = rightExpressionValue.UnresolvedValue;
                    result.UnresolvedFormat = leftExpressionValue.TextResult + "{0}";
                }
                if (!rightExpressionValue.IsSolved && !leftExpressionValue.IsSolved)
                {
                    result.CanBeUsedAsQuery = false;
                    result.UnresolvableReason = $"{leftExpressionValue.UnresolvableReason}{rightExpressionValue.UnresolvableReason}";
                }

                return result;
            }
        }
    }
}
