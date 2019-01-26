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
    public class SimpleMemberAccessStrategy : IResolveStrategy
    {
        public ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance)
        {
            var simpleMemberAccessExpression = expressionSyntax as MemberAccessExpressionSyntax;
            if (simpleMemberAccessExpression.Expression is TypeOfExpressionSyntax && simpleMemberAccessExpression.Name.Identifier.ValueText == "Name")
            {
                var typeOfExpression = simpleMemberAccessExpression.Expression as TypeOfExpressionSyntax;
                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Non compile-time variable used - typeof(T).Name",
                    CanBeUsedAsQuery = true,
                    ExpressionText = expressionSyntax.ToString(),
                    TextResult = (typeOfExpression.Type as IdentifierNameSyntax).Identifier.ValueText,
                    UnresolvedPart = UnresolvedPartType.GenericClassName,
                    UnresolvedValue = (typeOfExpression.Type as IdentifierNameSyntax).Identifier.ValueText,
                };
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
