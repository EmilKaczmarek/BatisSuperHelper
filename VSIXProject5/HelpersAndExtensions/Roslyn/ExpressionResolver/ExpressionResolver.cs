using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver
{
    public class ExpressionResolver
    {
        private static readonly IReadOnlyDictionary<SyntaxKind, IResolveStrategy> _strategies 
            = new Dictionary<SyntaxKind, IResolveStrategy>
              {
                  {SyntaxKind.AddExpression, new AddStrategy()},
                  {SyntaxKind.IdentifierName, new IdentifierStrategy() },
                  {SyntaxKind.InterpolatedStringExpression, new InterpolatedStringStrategy() },
                  {SyntaxKind.InvocationExpression, new InvocationStrategy() },
                  {SyntaxKind.SimpleMemberAccessExpression, new SimpleMemberAccessStrategy() },
                  {SyntaxKind.StringLiteralExpression, new StringLiteralStrategy() }
              };

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

        private MethodInfo ResolveToClassAndMethodNames(SyntaxNode node, SemanticModel semModel)
        {
            var analyzeNode = node as SyntaxNode;
            while (analyzeNode != null && !analyzeNode.IsKind(SyntaxKind.MethodDeclaration))
            {   
                analyzeNode = analyzeNode.Parent;
            }

            if (analyzeNode == null)
                return null;

            var methodDeclaration = analyzeNode as MethodDeclarationSyntax;
            if (methodDeclaration.Modifiers.Any(e => e.Text == "public"))
            {
                var classDeclaration = analyzeNode.Parent as ClassDeclarationSyntax;

                return new MethodInfo
                {
                    MethodName = methodDeclaration.Identifier.Text,
                    MethodClass = classDeclaration.Identifier.Text,
                }; 
            }
            return null;
        }

        public ExpressionResult GetStringValueOfExpression(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _callStackNum++;
            if (_callStackNum >= _maxCallStackNum)
                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Call stack extended",
                    CallsNeeded = _callStackNum,
                    TextResult = "",
                }
                .WithNodeInfo(new NodeInfo
                {
                    FileName = Path.GetFileName(document.FilePath),
                    ProjectName = document.Project.Name,
                    FilePath = document.FilePath,
                    DocumentId = document.Id,
                });

            if (expressionSyntax == null)
                return new ExpressionResult
                {
                    IsSolved = false,
                    UnresolvableReason = "Provided expression is null",
                    CallsNeeded = _callStackNum,
                    TextResult = "",
                }
                .WithNodeInfo(new NodeInfo
                {
                    FileName = Path.GetFileName(document.FilePath),
                    ProjectName = document.Project.Name,
                    FilePath = document.FilePath,
                    DocumentId = document.Id,
                });

            if (_strategies.TryGetValue(expressionSyntax.Kind(), out var strategy))
            {
                //var allSymbolsForSpan = nodes.Select(e => semanticModel.GetSymbolInfo(e)).Where(e=>e.Symbol !=null).ToList();
                var symbolInfo = semanticModel.GetSymbolInfo(expressionSyntax.Parent);

                return strategy.Resolve(document, expressionSyntax, nodes, semanticModel, this)
                    .WithNodeInfo(new NodeInfo
                    {
                        MethodInfo = ResolveToClassAndMethodNames(expressionSyntax, semanticModel),
                        FileName = Path.GetFileName(document.FilePath),
                        LineNumber = expressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        ProjectName = document.Project.Name,
                        FilePath = document.FilePath,
                        DocumentId = document.Id,
                    })
                .WithCallStackNumber(_callStackNum);
            }

            return new ExpressionResult
            {
                IsSolved = false,
                UnresolvableReason = "Not supported SyntaxKind",
                CallsNeeded = _callStackNum,
                TextResult = "",
            }
            .WithNodeInfo(new NodeInfo
            {
                MethodInfo = ResolveToClassAndMethodNames(expressionSyntax, semanticModel),
                FileName = Path.GetFileName(document.FilePath),
                LineNumber = expressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                ProjectName = document.Project.Name,
                FilePath = document.FilePath,
                DocumentId = document.Id,
            })
            .WithCallStackNumber(_callStackNum);
        }  
    }
}
