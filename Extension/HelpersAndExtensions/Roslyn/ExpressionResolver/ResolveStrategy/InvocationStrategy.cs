using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Profiling;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class InvocationStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            using (MiniProfiler.Current.Step(nameof(InvocationStrategy)))
            {
                var invocationExpression = expressionSyntax as InvocationExpressionSyntax;
                var optionalConst = semanticModel.GetConstantValue(expressionSyntax);
                if (optionalConst.HasValue)
                {
                    return new ExpressionResult
                    {
                        CanBeUsedAsQuery = false,
                        ExpressionText = expressionSyntax.ToString(),
                        IsSolved = true,
                        TextResult = optionalConst.Value.ToString(),
                    };
                }
                var nextInvocationExpression = invocationExpression.Expression;
                if (nextInvocationExpression is MemberAccessExpressionSyntax)
                {
                    var nextNextInvocationExpression = nextInvocationExpression as MemberAccessExpressionSyntax;
                    var predefinedTypeExpression = nextNextInvocationExpression.Expression as PredefinedTypeSyntax;
                    var indentifierName = nextNextInvocationExpression.Name as IdentifierNameSyntax;
                    if (predefinedTypeExpression?.Keyword.ValueText == "string" && indentifierName?.Identifier.ValueText == "Format")
                    {
                        string format = "";
                        if (invocationExpression.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax)
                        {
                            format = (invocationExpression.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax).Token.ValueText;
                        }
                        List<string> parameters = new List<string>();
                        List<ExpressionResult> expressionResolverResults = new List<ExpressionResult>();
                        foreach (var parameter in invocationExpression.ArgumentList.Arguments.Skip(1))
                        {
                            expressionResolverResults.Add(expressionResolverInstance.GetStringValueOfExpression(document, parameter.Expression, nodes, semanticModel));
                        }

                        if (expressionResolverResults.Any(e => !e.IsSolved && e.CanBeUsedAsQuery))
                        {
                            return new ExpressionResult
                            {
                                CanBeUsedAsQuery = !(expressionResolverResults.Count(e => !e.IsSolved && e.CanBeUsedAsQuery) > 1),//Atm, only allow for 1 variable
                                ExpressionText = expressionSyntax.ToString(),
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
                            ExpressionText = expressionSyntax.ToString(),
                            IsSolved = true,
                            TextResult = string.Format(format, expressionResolverResults.Select(e => e.TextResult).ToArray()),
                        };
                    }
                }
                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Unknow method.",
                    TextResult = "",
                };
            }
        }
    }
}
