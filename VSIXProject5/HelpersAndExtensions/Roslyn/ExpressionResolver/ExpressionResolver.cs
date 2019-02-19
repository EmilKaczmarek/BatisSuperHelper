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
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver
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
        private int _maxCallStackNumAtStart;

        public ExpressionResolver()
        {
            _maxCallStackNum = 100;
            _maxCallStackNumAtStart = _maxCallStackNum;
        }
        public ExpressionResolver(int maxCallStack)
        {
            _maxCallStackNum = maxCallStack;
            _maxCallStackNumAtStart = _maxCallStackNum;
        }
        private int _callStackNum = 1;

        private MethodInfo ResolveToClassAndMethodNames(SyntaxNode node, SemanticModel semModel)
        {
            var analyzeNode = node;
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

        private ExpressionResult GetResultForCallStackExtended(Document document)
        {
            ExpressionResult result = new ExpressionResult
            {
                IsSolved = false,
                UnresolvableReason = "Call stack extended",
                CallsNeeded = _callStackNum,
                TextResult = "",
            }
            .WithCallStackNumber(_callStackNum);

            return document != null
                ? result.WithNodeInfo(new NodeInfo
                {
                    FileName = Path.GetFileName(document.FilePath),
                    ProjectName = document.Project.Name,
                    FilePath = document.FilePath,
                    DocumentId = document.Id,
                })
                : result;
        }

        private ExpressionResult GetResultForNullExpression(Document document)
        {
            ExpressionResult result = new ExpressionResult
            {
                IsSolved = false,
                UnresolvableReason = "Provided expression is null",
                CallsNeeded = _callStackNum,
                TextResult = "",
            }
            .WithCallStackNumber(_callStackNum);

            return document !=null
               ? result.WithNodeInfo(new NodeInfo
               {
                   FileName = Path.GetFileName(document.FilePath),
                   ProjectName = document.Project.Name,
                   FilePath = document.FilePath,
                   DocumentId = document.Id,
               })
               : result;
        }

        private ExpressionResult GetResultForKindNotSupported(Document document, ExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            var result = new ExpressionResult
            {
                IsSolved = false,
                UnresolvableReason = "Not supported SyntaxKind",
                CallsNeeded = _callStackNum,
                TextResult = "",
            }
            .WithCallStackNumber(_callStackNum);
            return document != null || expressionSyntax !=null || semanticModel !=null
              ? result.WithNodeInfo(new NodeInfo
              {
                  MethodInfo = ResolveToClassAndMethodNames(expressionSyntax, semanticModel),
                  FileName = Path.GetFileName(document.FilePath),
                  LineNumber = expressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                  ProjectName = document.Project.Name,
                  FilePath = document.FilePath,
                  DocumentId = document.Id,
              })
              : result;
        }

        public ExpressionResult GetStringValueOfExpression(ExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            return GetStringValueOfExpression(null, expressionSyntax, semanticModel.SyntaxTree.GetRoot().DescendantNodes(), semanticModel);
        }

        public ExpressionResult GetStringValueOfExpression(Document document, ExpressionSyntax expressionSyntax, IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel)
        {
            _callStackNum++;
            if (_callStackNum >= _maxCallStackNum)
                return GetResultForCallStackExtended(document);

            if (expressionSyntax == null)
                return GetResultForNullExpression(document);

            if (_strategies.TryGetValue(expressionSyntax.Kind(), out var strategy))
            {
                _maxCallStackNum = expressionSyntax.IsKind(SyntaxKind.IdentifierName)? 10 : _maxCallStackNumAtStart;

                var result = strategy.Resolve(document, expressionSyntax, nodes, semanticModel, this).WithCallStackNumber(_callStackNum);
                return document != null
                    ?result.WithNodeInfo(new NodeInfo
                        {
                            MethodInfo = ResolveToClassAndMethodNames(expressionSyntax, semanticModel),
                            FileName = Path.GetFileName(document.FilePath),
                            LineNumber = expressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            ProjectName = document.Project.Name,
                            FilePath = document.FilePath,
                            DocumentId = document.Id,
                        })
                    :result;
            }

            return GetResultForKindNotSupported(document, expressionSyntax, semanticModel);
        }  
    }
}
