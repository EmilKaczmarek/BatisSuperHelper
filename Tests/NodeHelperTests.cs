using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IBatisSuperHelper.Helpers;

namespace Tests
{
    [TestClass]
    public class NodeHelperTests
    {
        [TestMethod]
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

            Assert.AreEqual("int", genericName);
        }

        [TestMethod]
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

            Assert.AreEqual("Int32", genericName);
        }

    }
}
