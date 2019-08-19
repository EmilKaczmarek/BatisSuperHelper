using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Profiling;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy.KnownMethodResolvers;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class InvocationStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            using (MiniProfiler.Current.Step(nameof(InvocationStrategy)))
            {
                var invocationExpression = expressionSyntax as InvocationExpressionSyntax;
                var optionalConst = semanticModel?.GetConstantValue(expressionSyntax);
                if (optionalConst?.HasValue ?? false)
                {
                    return new ExpressionResult
                    {
                        CanBeUsedAsQuery = false,
                        ExpressionText = expressionSyntax.ToString(),
                        IsSolved = true,
                        TextResult = optionalConst.Value.Value.ToString(),
                    };
                }
                var nextInvocationExpression = invocationExpression.Expression;
                if (nextInvocationExpression is MemberAccessExpressionSyntax)
                {
                    var knowMethodFactory = new KnownMethodResolverFactory(invocationExpression, nextInvocationExpression, expressionResolverInstance, document, nodes, semanticModel);
                    return knowMethodFactory.GetResolver().Resolve();
                }

                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = $"Child expression is not {nameof(MemberAccessExpressionSyntax)}",
                    TextResult = "",
                };
            }
        }
    }
}
