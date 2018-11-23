using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions.Roslyn
{
    public class ExpressionResolver
    {
        private int _callStackNum = 1;
        public string GetStringValueOfExpression(ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _callStackNum++;
            if (_callStackNum >= 1003)
                return "";

            if (expressionSyntax == null)
                return "";

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
                        return GetStringValueOfExpression(leftExpression, nodes, semanticModel) + GetStringValueOfExpression(rightExpression, nodes, semanticModel);
                    }
                case SyntaxKind.StringLiteralExpression:
                    {
                        var token = (expressionSyntax as LiteralExpressionSyntax).Token;
                        var tokenValue = token.Value;
                        return token.ValueText;
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        var invocationExpression = expressionSyntax as InvocationExpressionSyntax;
                        var optionalConst = semanticModel.GetConstantValue(expressionSyntax);
                        if (optionalConst.HasValue)
                        {
                            return optionalConst.Value.ToString();
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
                                foreach (var parameter in invocationExpression.ArgumentList.Arguments.Skip(1))
                                {
                                    parameters.Add(GetStringValueOfExpression(parameter.Expression, nodes, semanticModel));
                                }
                                return string.Format(format, parameters.ToArray());
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
                            return (typeOfExpression.Type as IdentifierNameSyntax).Identifier.ValueText;
                        }
                        break;
                    }
                default:
                    break;
            }
            return "";
        }

        public string GetStringValueFromInterpolation(IEnumerable<SyntaxNode> nodes, InterpolatedStringExpressionSyntax interpolatedNode, SemanticModel semanticModel)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var content in interpolatedNode.Contents)
            {
                if (content is InterpolationSyntax)
                {
                    var expression = (content as InterpolationSyntax).Expression;
                    sb.Append(GetStringValueOfExpression(expression, nodes, semanticModel));
                }
                if (content is InterpolatedStringTextSyntax)
                {
                    sb.Append((content as InterpolatedStringTextSyntax).TextToken.Text);
                }
            }

            return sb.ToString();
        }
    }
}
