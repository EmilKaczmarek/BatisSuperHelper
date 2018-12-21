using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.HelpersAndExtensions.Roslyn
{
    public class ExpressionResolver
    {
        private int _maxCallStackNum;
        public ExpressionResolver()
        {
            _maxCallStackNum = 1000;
        }
        public ExpressionResolver(int maxCallStack)
        {
            _maxCallStackNum = maxCallStack;
        }
        private int _callStackNum = 1;

        private string ResolveToClassAndMethodNames(SyntaxNode node)
        {
            var analyzeNode = node as SyntaxNode;
            string className;
            string methodName;
            while (!analyzeNode.IsKind(SyntaxKind.MethodDeclaration))
            {
                analyzeNode = analyzeNode.Parent;            
            }

            var methodDeclaration = analyzeNode as MethodDeclarationSyntax;
            if (methodDeclaration.Modifiers.Any(e => e.Text == "public"))
            {
                methodName = methodDeclaration.Identifier.Text;
                return methodName;

                var classDeclaration = analyzeNode.Parent as ClassDeclarationSyntax;
                className = classDeclaration.Identifier.Text; 
            }
            return null;
        }

        public ExpressionResult GetStringValueOfExpression(ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _callStackNum++;
            if (_callStackNum >= _maxCallStackNum)
                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Call stack extended",
                    CallsNeeded = _callStackNum,
                    TextResult = "",
                };

            if (expressionSyntax == null)
                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Provided expression is null",
                    CallsNeeded = _callStackNum,
                    TextResult = "",
                }; 

            var argumentExpressionKind = expressionSyntax.Kind();
            switch (argumentExpressionKind)
            {
                case SyntaxKind.InterpolatedStringExpression:
                    {
                        return GetStringValueFromInterpolation(nodes, expressionSyntax as InterpolatedStringExpressionSyntax, semanticModel);
                    }
                case SyntaxKind.IdentifierName:
                    {
                        var identifierNameSyntax = expressionSyntax as IdentifierNameSyntax;
                        var descendantTest = identifierNameSyntax.DescendantNodesAndSelf();
                        var variableDeclaration = nodes.OfType<VariableDeclarationSyntax>().SelectMany(e => e.Variables.Where(x => x.Identifier.Text == identifierNameSyntax.Identifier.Text)).FirstOrDefault();
                        if (variableDeclaration == null)//If no Variable of matching name was found, than look in properties.
                        {
                            var property = nodes.OfType<PropertyDeclarationSyntax>().FirstOrDefault(e => e.Identifier.ValueText == identifierNameSyntax.Identifier.ValueText);
                            if (property?.ExpressionBody != null)
                            {
                                return GetStringValueOfExpression(property.ExpressionBody.Expression, nodes, semanticModel);
                            }
                            else if (property?.AccessorList != null)
                            {
                                var getter = property.AccessorList.Accessors.FirstOrDefault(e => e.IsKind(SyntaxKind.GetAccessorDeclaration));
                                var getterBody = getter.Body;
                                var firstReturnStatement = getterBody.Statements.FirstOrDefault(e => e.IsKind(SyntaxKind.ReturnStatement)) as ReturnStatementSyntax;
                                return GetStringValueOfExpression(firstReturnStatement.Expression, nodes, semanticModel);
                            }
                        }
                        else
                        {
                            var initializerValue = variableDeclaration.Initializer?.Value;
                            return GetStringValueOfExpression(initializerValue, nodes, semanticModel);
                        }
                        break;
                    }
                case SyntaxKind.AddExpression:
                    {
                        var binaryExpressionNode = expressionSyntax as BinaryExpressionSyntax;
                        var leftExpression = binaryExpressionNode.Left;
                        var rightExpression = binaryExpressionNode.Right;
                        var leftExpressionValue = GetStringValueOfExpression(leftExpression, nodes, semanticModel);
                        var rightExpressionValue = GetStringValueOfExpression(rightExpression, nodes, semanticModel);
                        if (leftExpressionValue.IsSolved && rightExpressionValue.IsSolved)
                        {
                            return new ExpressionResult
                            {
                                IsSolved = true,
                                CallsNeeded = _callStackNum,
                                CanBeUsedAsQuery = false,
                                ExpressionText = expressionSyntax.ToString(),
                                TextResult = leftExpressionValue.TextResult + rightExpressionValue.TextResult,
                            };
                        }
                        var result = new ExpressionResult
                        {
                            IsSolved = false,
                            CallsNeeded = _callStackNum,
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
                            result.MethodName = ResolveToClassAndMethodNames(expressionSyntax);
                        }

                        if (!rightExpressionValue.IsSolved)
                        {
                            result.TextResult += rightExpressionValue.TextResult;
                            result.CanBeUsedAsQuery = rightExpressionValue.CanBeUsedAsQuery;
                            result.UnresolvableReason = rightExpressionValue.UnresolvableReason;
                            result.UnresolvedPart = rightExpressionValue.UnresolvedPart;
                            result.UnresolvedValue = rightExpressionValue.UnresolvedValue;
                            result.UnresolvedFormat = leftExpressionValue.TextResult + "{0}";
                            result.MethodName = ResolveToClassAndMethodNames(expressionSyntax);
                        }
                        if (!rightExpressionValue.IsSolved && !leftExpressionValue.IsSolved)
                        {
                            result.CanBeUsedAsQuery = false;
                            result.UnresolvableReason = $"{leftExpressionValue.UnresolvableReason}{rightExpressionValue.UnresolvableReason}";
                            result.MethodName = ResolveToClassAndMethodNames(expressionSyntax);
                        }

                        return result;
                    }
                case SyntaxKind.StringLiteralExpression:
                    {
                        var token = (expressionSyntax as LiteralExpressionSyntax).Token;
                        var tokenValue = token.Value;
                        return new ExpressionResult
                        {
                            CallsNeeded = _callStackNum,
                            CanBeUsedAsQuery = false,
                            ExpressionText = expressionSyntax.ToString(),
                            IsSolved = true,
                            TextResult = token.ValueText,
                        };
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        var invocationExpression = expressionSyntax as InvocationExpressionSyntax;
                        var optionalConst = semanticModel.GetConstantValue(expressionSyntax);
                        if (optionalConst.HasValue)
                        {
                            return new ExpressionResult
                            {
                                CallsNeeded = _callStackNum,
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
                                    expressionResolverResults.Add(GetStringValueOfExpression(parameter.Expression, nodes, semanticModel));
                                }

                                if (expressionResolverResults.Any(e => !e.IsSolved && e.CanBeUsedAsQuery))
                                {
                                    return new ExpressionResult
                                    {
                                        CallsNeeded = _callStackNum,
                                        CanBeUsedAsQuery = !(expressionResolverResults.Count(e => !e.IsSolved && e.CanBeUsedAsQuery) > 1),//Atm, only allow for 1 variable
                                        ExpressionText = expressionSyntax.ToString(),
                                        IsSolved = false,
                                        TextResult = string.Format(format, expressionResolverResults.Select(e => e.TextResult).ToArray()),
                                        UnresolvableReason = expressionResolverResults.First(e => !e.IsSolved).TextResult,
                                        UnresolvedPart = expressionResolverResults.First(e => !e.IsSolved).UnresolvedPart,
                                        UnresolvedValue = expressionResolverResults.First(e => !e.IsSolved).UnresolvedValue,
                                        UnresolvedFormat = string.Join("", expressionResolverResults.Select(e => e.IsSolved ? e.TextResult : "{0}")),
                                        MethodName = ResolveToClassAndMethodNames(expressionSyntax),
                                    };
                                }
                                return new ExpressionResult
                                {
                                    CallsNeeded = _callStackNum,
                                    CanBeUsedAsQuery = false,
                                    ExpressionText = expressionSyntax.ToString(),
                                    IsSolved = true,
                                    TextResult = string.Format(format, expressionResolverResults.Select(e => e.TextResult).ToArray()),
                                };
                            }
                        }
                        break;
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var simpleMemberAccessExpression = expressionSyntax as MemberAccessExpressionSyntax;
                        if (simpleMemberAccessExpression.Expression is TypeOfExpressionSyntax && simpleMemberAccessExpression.Name.Identifier.ValueText == "Name")
                        {
                            var typeOfExpression = simpleMemberAccessExpression.Expression as TypeOfExpressionSyntax;
                            var testType = typeOfExpression.GetType();
                            return new ExpressionResult
                            {
                                IsSolved = false,
                                UnresolvableReason = "Non compile-time variable used - typeof(T).Name",
                                CanBeUsedAsQuery = true,
                                CallsNeeded = _callStackNum,
                                ExpressionText = expressionSyntax.ToString(),
                                TextResult = (typeOfExpression.Type as IdentifierNameSyntax).Identifier.ValueText,
                                UnresolvedPart = UnresolvedPartType.ClassName,
                                UnresolvedValue = (typeOfExpression.Type as IdentifierNameSyntax).Identifier.ValueText,
                                MethodName = ResolveToClassAndMethodNames(expressionSyntax),
                            };
                        }
                        break;
                    }
                default:
                    break;
            }
            return new ExpressionResult
            {
                IsSolved = false,
                UnresolvableReason = "Not supported SyntaxKind",
                CallsNeeded = _callStackNum,
                TextResult = "",
                MethodName = ResolveToClassAndMethodNames(expressionSyntax),
            };
        }

        public ExpressionResult GetStringValueFromInterpolation(IEnumerable<SyntaxNode> nodes, InterpolatedStringExpressionSyntax interpolatedNode, SemanticModel semanticModel)
        {
            List<ExpressionResult> expressionResolverResults = new List<ExpressionResult>();
            foreach (var content in interpolatedNode.Contents)
            {
                if (content is InterpolationSyntax)
                {
                    var expression = (content as InterpolationSyntax).Expression;
                    expressionResolverResults.Add(GetStringValueOfExpression(expression, nodes, semanticModel));

                }
                if (content is InterpolatedStringTextSyntax)
                {
                    expressionResolverResults.Add(new ExpressionResult
                    {
                        IsSolved = true,
                        CallsNeeded = _callStackNum,
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
                    CallsNeeded = _callStackNum,
                    CanBeUsedAsQuery = !(expressionResolverResults.Count(e => !e.IsSolved && e.CanBeUsedAsQuery) > 1),//Atm, only allow for 1 variable
                    ExpressionText = interpolatedNode.ToString(),
                    IsSolved = false,
                    TextResult = string.Join("", expressionResolverResults.Select(e => e.TextResult)),
                    UnresolvableReason = expressionResolverResults.First(e => !e.IsSolved).TextResult,
                    UnresolvedPart = expressionResolverResults.First(e => !e.IsSolved).UnresolvedPart,
                    UnresolvedValue = expressionResolverResults.First(e => !e.IsSolved).UnresolvedValue,
                    UnresolvedFormat = string.Join("", expressionResolverResults.Select(e => e.IsSolved ? e.TextResult : "{0}")),
                    MethodName = ResolveToClassAndMethodNames(interpolatedNode),
                };
            }
            return new ExpressionResult
            {
                CallsNeeded = _callStackNum,
                CanBeUsedAsQuery = false,
                ExpressionText = string.Join("", expressionResolverResults.Select(e => e.TextResult)),
                IsSolved = true,
                TextResult = string.Join("", expressionResolverResults.Select(e => e.TextResult)),
            };
        }
    }
}
