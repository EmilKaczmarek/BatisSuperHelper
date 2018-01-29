﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace iBatisSuperHelper.Services.Helpers
{
    public class NodeHelpers
    {
        private readonly SemanticModel _semanticModel;
        public NodeHelpers(SemanticModel semanticModelForDocument)
        {
            _semanticModel = semanticModelForDocument;
        }
        ~NodeHelpers()
        {
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
                .Where(x => x != null && x.ContainingNamespace != null);
            return candidates.Any(x => x.ContainingNamespace.ToDisplayString().Contains("Batis"));
        }
        /// <summary>
        /// Returns IEnumerable with ArgumentLists that has non empty Argument member.
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        /// <returns></returns>
        private IEnumerable<ArgumentListSyntax> GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(IEnumerable<SyntaxNode> SyntaxNodes)
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
        private ArgumentListSyntax GetProperArgumentSyntaxNode(IEnumerable<ArgumentListSyntax> ArgumentListSyntaxNodes)
        {
            //Posibly not enough to determine if this is proper iBatis method.
            //Placeholder for handling other iBatis method.
            return ArgumentListSyntaxNodes.FirstOrDefault(e => e.Arguments.Count > 1) as ArgumentListSyntax;
        }
        /// <summary>
        /// Looks for Argument of type String, and returns it.
        /// </summary>
        /// <param name="argumentList"></param>
        /// <returns></returns>
        private ArgumentSyntax GetArgumentSyntaxOfStringType(ArgumentListSyntax argumentList)
        {
            ISymbol stringISymbol= _semanticModel.Compilation.GetTypeByMetadataName(typeof(String).FullName);
            var arguments = argumentList.Arguments;
            foreach(var argument in arguments)
            {
                var childNodes = argument.ChildNodes();
                var firstChildNode = childNodes.First();
                var testGetTypeInfo = _semanticModel.GetTypeInfo(firstChildNode);
                var type = testGetTypeInfo.Type;
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
                    if (testGetTypeInfo.ConvertedType.Name.Equals("Func")){
                        //this is Function! now, lets test if this contains iBatis querry
                        var nodes = argument.DescendantNodes();
                        var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(nodes);
                        var properNodes = GetProperArgumentSyntaxNode(syntaxArguments);
                        return GetArgumentSyntaxOfStringType(properNodes);//recursive ftw
                    }
                }
            }
            return null;
            //var f1 = argumentList.Arguments;
            //return argumentList.Arguments
            //    .FirstOrDefault(x => 
            //        _semanticModel.GetTypeInfo(x.ChildNodes().First()).Type.Equals(stringISymbol)
            //    );
        }
        /// <summary>
        /// Get text presentation of Query argument value
        /// </summary>
        /// <param name="SyntaxNodes"></param>
        public String GetQueryStringFromSyntaxNodes(IEnumerable<SyntaxNode> SyntaxNodes)
        {
            if (IsAnySyntaxNodeContainIBatisNamespace(SyntaxNodes))
            {
                var syntaxArguments = GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(SyntaxNodes);
                var singleArgumentListSyntax = GetProperArgumentSyntaxNode(syntaxArguments);
                var queryArgument = GetArgumentSyntaxOfStringType(singleArgumentListSyntax);
                return queryArgument.ToString().Replace("\"", "").Trim();
            }
            return null;
        }
    }
}