using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public class IdentifierStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
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
                else if (property?.AccessorList != null)
                {
                    var getter = property.AccessorList.Accessors.FirstOrDefault(e => e.IsKind(SyntaxKind.GetAccessorDeclaration));
                    var getterBody = getter.Body;
                    var firstReturnStatement = getterBody.Statements.FirstOrDefault(e => e.IsKind(SyntaxKind.ReturnStatement)) as ReturnStatementSyntax;
                    return expressionResolverInstance.GetStringValueOfExpression(document, firstReturnStatement.Expression, nodes, semanticModel);
                }
            }

            var initializerValue = variableDeclaration?.Initializer?.Value;
            return expressionResolverInstance.GetStringValueOfExpression(document, initializerValue, nodes, semanticModel);
        }
    }
}
