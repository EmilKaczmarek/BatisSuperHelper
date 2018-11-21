﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VSIXProject5.HelpersAndExtensions.Roslyn;

namespace VSIXProject5.Helpers
{
    public class NodeHelpers
    {
        public readonly SemanticModel _semanticModel;
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
        //Method is working 100% correct, but could lead to unwanted behaviour.
        //Should work on better method to determine if it's using iBatis.
        /// <summary>
        /// Looks for all nodes that has 'Batis' in namespace.
        /// Used to determine if method/class etc is iBatis in iBatis namespace.
        /// </summary>
        /// <param name="SyntaxNodes">Syntax Nodes</param>
        /// <returns></returns>
        public bool IsAnySyntaxNodeContainIBatisNamespace(IEnumerable<SyntaxNode> SyntaxNodes)
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
            //Posibly not enough to determine if this is proper iBatis method.
            //Placeholder for handling other iBatis method.
            //But what other factors could be used here?
            return ArgumentListSyntaxNodes.FirstOrDefault(e => e.Arguments.Count > 1) as ArgumentListSyntax;
        }
        /// <summary>
        /// Looks for Argument of type String, and returns it.
        /// </summary>
        /// <param name="argumentList"></param>
        /// <returns></returns>
        public ArgumentSyntax GetArgumentSyntaxOfStringType(ArgumentListSyntax argumentList)
        {
            ISymbol stringISymbol= _semanticModel.Compilation.GetTypeByMetadataName(typeof(String).FullName);
            var arguments = argumentList.Arguments;
            foreach(var argument in arguments)
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
                    if (typeInfo.ConvertedType.Name.Equals("Func")){
                        //this is Function! now, lets test if this contains iBatis querry
                        var nodes = argument.DescendantNodes();
                        var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(nodes);
                        var properNodes = GetProperArgumentSyntaxNode(syntaxArguments);
                        return GetArgumentSyntaxOfStringType(properNodes);//recursive ftw
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Get text presentation of Query argument value
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        [Obsolete("Old logic for expression analysis. Use overload instead", false)]
        public String GetQueryStringFromSyntaxNodes(IEnumerable<SyntaxNode> SyntaxNodes)
        {
            if (IsAnySyntaxNodeContainIBatisNamespace(SyntaxNodes))
            {
                var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(SyntaxNodes);
                var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);

                var queryArgument = GetArgumentSyntaxOfStringType(singleArgumentListSyntax);
                var constantValue = _semanticModel.GetConstantValue(queryArgument.Expression).Value;
                
                return constantValue != null?constantValue.ToString(): queryArgument.ToString().Replace("\"", "").Trim();//TODO: Use scripting to handle even more crazy cases.
            }
            return null;
        }

        /// <summary>
        /// Get text presentation of Query argument expression if possible
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        /// <param name="allDocumentNodes"></param>
        public String GetQueryStringFromSyntaxNodes(IEnumerable<SyntaxNode> SyntaxNodes, IEnumerable<SyntaxNode> allDocumentNodes)
        {
            if (IsAnySyntaxNodeContainIBatisNamespace(SyntaxNodes))
            {
                var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(SyntaxNodes);
                var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);

                var queryArgument = GetArgumentSyntaxOfStringType(singleArgumentListSyntax);

                return new ExpressionResolver().GetStringValueOfExpression(queryArgument.Expression, allDocumentNodes, _semanticModel);
            }
            return null;
        }

        public ArgumentSyntax GetProperArgumentNodeInNodes(IEnumerable<SyntaxNode> nodes)
        {
            var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(nodes);
            var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);
            return GetArgumentSyntaxOfStringType(singleArgumentListSyntax);
        }
    }
}
