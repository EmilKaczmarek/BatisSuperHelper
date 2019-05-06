using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using IBatisSuperHelper.Helpers;
using Xunit;

namespace Tests
{
    public class NodeHelperTests
    {
        [Fact]
        public void GenericClassNameOneLiner()
        {
            var code = @"using System;
using System.Collections;
class C 
{
  void M()
  {
    var test = new List<int>();
  }
}
";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create("HelloWorld")
                                   .AddReferences(
                                        MetadataReference.CreateFromFile(
                                            typeof(object).Assembly.Location))
                                   .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);

            var span = tree.GetText().Lines[6].Span;
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var allNodes = root.DescendantNodesAndSelf();
            var lineNodes = root.DescendantNodesAndSelf(span);

            var nodeHelperInstance = new NodeHelpers(model);
            var genericName = nodeHelperInstance.GetClassNameUsedAsGenericParameter(lineNodes, allNodes);

            Assert.Equal("int", genericName);
        }

        [Fact]
        public void GenericClassNameWithoutDirectGenericNameSyntaxInLine()
        {
            var code = @"using System;
using System.Collections.Generic;
class C 
{
  void M()
  {
    var test = new List<int>();
    test.Add(0);
  }
}
";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("HelloWorld")
                                   .AddReferences(
                                        MetadataReference.CreateFromFile(
                                            typeof(object).Assembly.Location))
                                   .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);

            var span = tree.GetText().Lines[7].Span;
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var allNodes = root.DescendantNodesAndSelf();
            var lineNodes = root.DescendantNodesAndSelf(span);

            var nodeHelperInstance = new NodeHelpers(model);
            var genericName = nodeHelperInstance.GetClassNameUsedAsGenericParameter(lineNodes, allNodes);

            Assert.Equal("Int32", genericName);
        }

    }
}
