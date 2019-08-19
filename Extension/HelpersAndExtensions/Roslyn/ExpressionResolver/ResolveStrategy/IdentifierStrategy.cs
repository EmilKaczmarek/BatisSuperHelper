using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using StackExchange.Profiling;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class IdentifierStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            using (MiniProfiler.Current.Step(nameof(IdentifierStrategy)))
            {
                var identifierNameSyntax = expressionSyntax as IdentifierNameSyntax;
                var descendantTest = identifierNameSyntax.DescendantNodesAndSelf();
                var variableDeclaration = nodes.OfType<VariableDeclarationSyntax>().SelectMany(e => e.Variables.Where(x => x.Identifier.Text == identifierNameSyntax.Identifier.Text)).FirstOrDefault();
                if (variableDeclaration == null)//If no Variable of matching name was found, than look in properties.
                {
                    var property = nodes.OfType<PropertyDeclarationSyntax>().FirstOrDefault(e => e.Identifier.ValueText == identifierNameSyntax.Identifier.ValueText);
                    if (property?.ExpressionBody != null)
                    {
                        return expressionResolverInstance.GetStringValueOfExpression(document, property.ExpressionBody.Expression, nodes, semanticModel);
                    }
                    if (property?.AccessorList != null)
                    {
                        var getter = property.AccessorList.Accessors.FirstOrDefault(e => e.IsKind(SyntaxKind.GetAccessorDeclaration));
                        var getterBody = getter.Body;
                        if (getterBody == null)
                        {
                            return new ExpressionResult
                            {
                                IsSolved = false,
                                UnresolvableReason = "Getter Body is null",
                                TextResult = "",
                            };
                        }
                        var firstReturnStatement = getterBody.Statements.FirstOrDefault(e => e.IsKind(SyntaxKind.ReturnStatement)) as ReturnStatementSyntax;
                        return expressionResolverInstance.GetStringValueOfExpression(document, firstReturnStatement.Expression, nodes, semanticModel);
                    }

                }

                if (variableDeclaration?.Initializer != null)
                {
                    return expressionResolverInstance.GetStringValueOfExpression(document, variableDeclaration.Initializer.Value, nodes, semanticModel);
                }

                //Field/variable is in other document than currently analyzed
                if (identifierNameSyntax.SyntaxTree.FilePath != expressionSyntax.SyntaxTree.FilePath)
                {
                    return expressionResolverInstance.GetStringValueOfExpression(null, identifierNameSyntax, identifierNameSyntax.SyntaxTree.GetRoot().DescendantNodes(), null);
                }

                //Field/variable could still be in other document, but expression is not saying this. Unsafe 
                if (semanticModel != null)
                {
                    var symbol = semanticModel.GetSymbolInfo(identifierNameSyntax);
                    if (symbol.Symbol != null)
                    {
                        if (symbol.Symbol.Locations.Count() == 1 && identifierNameSyntax.SyntaxTree.FilePath != symbol.Symbol.Locations[0].SourceTree.FilePath)
                        {
                            return expressionResolverInstance.GetStringValueOfExpression(null, identifierNameSyntax, symbol.Symbol.Locations[0].SourceTree.GetRoot().DescendantNodes(), null);
                        }
                    }
                }

                var assigmentsInDocument = nodes.OfType<AssignmentExpressionSyntax>();
                var assigmentExpressions = assigmentsInDocument
                    .Where(e => (e.Left as IdentifierNameSyntax)?.Identifier.Text == identifierNameSyntax.Identifier.Text).Select(e => e.Right)
                    .Concat(
                        assigmentsInDocument
                        .Where(e => (e.Right as IdentifierNameSyntax)?.Identifier.Text == identifierNameSyntax.Identifier.Text).Select(e => e.Left)
                    );

                foreach (var expression in assigmentExpressions)
                {
                    var expressionResult = expressionResolverInstance.GetStringValueOfExpression(document, expression, nodes, semanticModel);
                    if (expressionResult.IsSolved || expressionResult.CanBeUsedAsQuery)
                        return expressionResult;
                }

                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Unable to find identifier initializer in document.",
                    TextResult = "",
                };
            }
        }
    }
}
