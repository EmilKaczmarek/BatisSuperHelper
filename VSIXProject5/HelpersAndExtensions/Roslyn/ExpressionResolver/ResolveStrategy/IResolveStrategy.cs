using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy
{
    public interface IResolveStrategy
    {
        ExpressionResult Resolve(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ExpressionResolver expressionResolverInstance); 
    }
}
