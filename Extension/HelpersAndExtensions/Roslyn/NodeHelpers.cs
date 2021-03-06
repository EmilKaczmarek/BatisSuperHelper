﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BatisSuperHelper.HelpersAndExtensions.Roslyn;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Storage;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy;
using BatisSuperHelper.Constants;
using BatisSuperHelper.Constants.BatisConstants;

namespace BatisSuperHelper.Helpers
{
    public class NodeHelpers
    {
        public readonly SemanticModel _semanticModel;

        public NodeHelpers()
        {

        }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="semanticModelForDocument">Roslyn semantic model of document</param>
        public NodeHelpers(SemanticModel semanticModelForDocument)
        {
            _semanticModel = semanticModelForDocument;
        }
        /// <summary>
        /// Try to returns Node that is Return Statment type.
        /// Used to determine if Document/Document fragment contains 'return' keyword.
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        /// <returns>Single SyntaxNode that is ReturnStatmentSyntax type or null</returns>
        public SyntaxNode GetFirstNodeOfReturnStatmentSyntaxType(IEnumerable<SyntaxNode> SyntaxNodes)
        {
            return SyntaxNodes.FirstOrDefault(x => x is ReturnStatementSyntax);
        }

        public bool TryGetArgumentNodeFromInvocation(InvocationExpressionSyntax node, int argumentIndex, out ExpressionSyntax argumentNode)
        {
            argumentNode = null;
            if (argumentIndex < 0 || (node.ArgumentList == null && argumentIndex + 1 > node.ArgumentList.Arguments.Count))
                return false;

            argumentNode = node.ArgumentList.Arguments[argumentIndex].Expression;
            return true;
        }

        public bool IsBatisMethod(InvocationExpressionSyntax analyzeNode)
        {
            MemberAccessExpressionSyntax nameSyntax = null;
            if (analyzeNode == null || analyzeNode.Expression == null)
                return false;
            if (analyzeNode.Expression is MemberAccessExpressionSyntax)
                nameSyntax = analyzeNode.Expression as MemberAccessExpressionSyntax;

            if (nameSyntax == null || nameSyntax.Name == null)
                return false;

            return CodeConstants.MethodNames.Contains(nameSyntax.Name.Identifier.ValueText);
        }

        public T GetFirstParentNodeOfType<T>(SyntaxNode node) where T: SyntaxNode
        {
            var analyzeNode = node;
            while (analyzeNode != null && analyzeNode.GetType() != typeof(T))
            {
                analyzeNode = analyzeNode.Parent;
            }
            return analyzeNode as T;
        }

        public bool DoesSyntaxNodeContainsBatisNamespace(SyntaxNode node)
        {
            var type = _semanticModel.GetTypeInfo(node);
            if (type.Type != null && type.Type.ContainingNamespace != null)
            {
                return type.Type.ContainingNamespace.ToDisplayString().Contains("Batis");
            }
            return false;
        }

        //Method is working 100% correct, but could lead to unwanted behaviour.
        //Should work on better method to determine if it's using Batis.
        /// <summary>
        /// Looks for all nodes that has 'Batis' in namespace.
        /// Used to determine if method/class etc is Batis in Batis namespace.
        /// </summary>
        /// <param name="SyntaxNodes">Syntax Nodes</param>
        /// <returns></returns>
        public bool IsAnySyntaxNodeContainBatisNamespace(IEnumerable<SyntaxNode> SyntaxNodes)
        {
            var candidates = SyntaxNodes
                .Select(x => _semanticModel.GetTypeInfo(x).Type)
                .Where(x => x != null && x.ContainingNamespace != null)
                .ToList();
            return candidates.Any(x => x.ContainingNamespace.ToDisplayString().Contains("Batis"));
        }

        /// <summary>
        /// Returns IEnumerable with ArgumentLists that has non empty Argument member.
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        /// <returns></returns>
        public IEnumerable<ArgumentListSyntax> GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(IEnumerable<SyntaxNode> SyntaxNodes)
        {
            return SyntaxNodes
                .OfType<ArgumentListSyntax>()
                .Where(x => x.Arguments.Any());
        }
        /// <summary>
        /// Gets ArgumentListSyntax for method that has proper argument number.
        /// </summary>
        /// <param name="ArgumentListSyntaxNodes"></param>
        /// <returns></returns>
        public ArgumentListSyntax GetProperArgumentSyntaxNode(IEnumerable<ArgumentListSyntax> ArgumentListSyntaxNodes)
        {
            //Posibly not enough to determine if this is proper Batis method.
            //Placeholder for handling other Batis method.
            //But what other factors could be used here?
            return ArgumentListSyntaxNodes.FirstOrDefault(e => e.Arguments.Count > 1);
        }
        /// <summary>
        /// Looks for Argument of type String, and returns it.
        /// </summary>
        /// <param name="argumentList"></param>
        /// <returns></returns>
        public ArgumentSyntax GetArgumentSyntaxOfStringType(ArgumentListSyntax argumentList)
        {
            if (argumentList == null)
                return null;

            ISymbol stringISymbol = _semanticModel.Compilation.GetTypeByMetadataName(typeof(String).FullName);
            var arguments = argumentList.Arguments;
            foreach (var argument in arguments)
            {
                var childNodes = argument.ChildNodes();
                var firstChildNode = childNodes.First();
                var typeInfo = _semanticModel.GetTypeInfo(firstChildNode);
                var type = typeInfo.Type;
                if (type != null)
                {
                    if (type.Equals(stringISymbol))
                    {
                        return argument;
                    }
                }
                else
                {
                    //NULL type? check maybe it's function/expression?
                    if (typeInfo.ConvertedType.Name.Equals("Func")) {
                        //this is Function! now, lets test if this contains Batis querry
                        var nodes = argument.DescendantNodes();
                        var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(nodes);
                        var properNodes = GetProperArgumentSyntaxNode(syntaxArguments);
                        return GetArgumentSyntaxOfStringType(properNodes);//recursive ftw
                    }
                }
            }
            return null;
        }

        public bool TryGetQueryStringFromStringLiteralNode(LiteralExpressionSyntax node, out string result)
        {
            var strategyResult =  new StringLiteralStrategy().Resolve(null, node, null, null, null);
            if (strategyResult.IsSolved)
            {
                result = strategyResult.TextResult;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Get text presentation of Query argument expression if possible
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        /// <param name="allDocumentNodes"></param>
        /// <param name="document"></param>
        public ExpressionResult GetQueryStringFromSyntaxNodes(Document document, IEnumerable<SyntaxNode> SyntaxNodes, IEnumerable<SyntaxNode> allDocumentNodes)
        {
            if (IsAnySyntaxNodeContainBatisNamespace(SyntaxNodes))
            {
                var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(SyntaxNodes);
                var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);

                var queryArgument = GetArgumentSyntaxOfStringType(singleArgumentListSyntax);
                if (queryArgument == null)
                {
                    var allArgumentSyntaxes = SyntaxNodes.OfType<ArgumentListSyntax>();
                    var oneArgumentSyntaxParent = allArgumentSyntaxes.First().Parent;
                    var resolveResult = GetMethodInfoForNode(oneArgumentSyntaxParent, _semanticModel);

                    var genericExists = GotoAsyncPackage.Storage.GenericMethods.TryGetValue(resolveResult, out ExpressionResult result);
                    if (genericExists)
                    {
                        return result;
                    }
                    return null;
                }
                return new ExpressionResolver().GetStringValueOfExpression(document, queryArgument?.Expression, allDocumentNodes, _semanticModel);
            }
            return null;
        }

        public SyntaxKind GetExpressionKindForBatisMethodArgument(IEnumerable<SyntaxNode> SyntaxNodes)
        {
            if (IsAnySyntaxNodeContainBatisNamespace(SyntaxNodes))
            {
                var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(SyntaxNodes);
                var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);

                var queryArgument = GetArgumentSyntaxOfStringType(singleArgumentListSyntax);

                if (queryArgument == null)
                    return SyntaxKind.None;

                var queryArgumentExpression = queryArgument.Expression;

                return queryArgumentExpression != null ? queryArgumentExpression.Kind() : SyntaxKind.None;
            }
            return SyntaxKind.None;
        }

        public ArgumentSyntax GetProperArgumentNodeInNodes(IEnumerable<SyntaxNode> nodes)
        {
            var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(nodes);
            var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);
            return GetArgumentSyntaxOfStringType(singleArgumentListSyntax);
        }

        public MethodInfo GetMethodInfoForNode(SyntaxNode node, SemanticModel semModel)
        {
            var analyzedNodeSymbolInfo = semModel.GetSymbolInfo(node);
            return new MethodInfo
            {
                MethodName = analyzedNodeSymbolInfo.Symbol.Name,
                MethodClass = analyzedNodeSymbolInfo.Symbol.ContainingSymbol.Name,
            };
        }
        public string GetClassNameUsedAsGenericParameter(IEnumerable<SyntaxNode> lineNodes, IEnumerable<SyntaxNode> documentNodes)
        {
            //Try get GenericNameSyntaxNodes in lineNodes
            var genericNameSyntax = lineNodes.OfType<GenericNameSyntax>();
            //There is GenericNameSyntax in line, so just get it...
            if (genericNameSyntax.Count() == 1)
            {
                var singleGenericNameSyntax = genericNameSyntax.First();
                return singleGenericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault() is PredefinedTypeSyntax firstArgument 
                    ? firstArgument.Keyword.ValueText 
                    : null;
            }
            //WTF
            if (genericNameSyntax.Count() > 1)
            {
                return null;
            }
            else
            {
                var identifierNames = lineNodes.OfType<IdentifierNameSyntax>();
                if (!identifierNames.Any()) return null;

                var allTypeInfos = identifierNames.Select(e => _semanticModel.GetTypeInfo(e)).ToList();

                TypeInfo? genericType = allTypeInfos.FirstOrDefault(e => e.Type != null && (e.Type as INamedTypeSymbol)?.IsGenericType != false);
                
                if (genericType != null)
                {
                    return (genericType.Value.Type as INamedTypeSymbol)?.TypeArguments.First().Name;
                }
            }
            

            return null;
        }
    }
}
