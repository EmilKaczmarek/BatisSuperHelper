﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using Xunit;

namespace Tests
{
    public class ExpressionResolverCrashTests
    {
        private readonly string _codeTemplateP1;
        private readonly string _codeTemplateP2;
        private readonly string _codeTemplateP3;
        private readonly string _codeTemplateP4;
        private readonly string _codeTemplateP5;

        public ExpressionResolverCrashTests()
        {
            //Uncomment this dummies if there are issues with loading assemblies.
            //using (var dummy = new Microsoft.CodeAnalysis.AdhocWorkspace())
            //{
            //}

            //var dummy2 = new Microsoft.CodeAnalysis.WorkspaceChangeKind();
            //var dummy3 = typeof(Microsoft.CodeAnalysis.CSharp.SyntaxFactory);

            _codeTemplateP1 = @"using System;
                using System.Collections;
                using System.Linq;
                using System.Text;
                 
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {                            
                            var nonDuplicateVariable = TestMethod(";

            _codeTemplateP2 = @","""");
                        ";

            _codeTemplateP3 = @"
                        }
                        public static string TestMethod(string one, string two)
                        {
                            return one + two;
                        }";
            _codeTemplateP4 = @"

                        public static string TestInReturn()
                        {
                            return UnknowMethod(";
            _codeTemplateP5 = @");
                        }
                    }
                }";
        }

        private string GenerateDocumentCodeFromTemplateUsingVariables(string insideMethodExpressionCode, string insideClassCode, string outsideClassCode, string inReturnUnknowMethodExpression)
        {
            return $"{_codeTemplateP1}{insideMethodExpressionCode}{_codeTemplateP2}{insideClassCode}{_codeTemplateP3}{outsideClassCode}{_codeTemplateP4}{inReturnUnknowMethodExpression}{_codeTemplateP5}";
        }

        private void PrepareAnalyzeModel(string documentCode, out Document document, out SemanticModel semanticModel,out IEnumerable<SyntaxNode> nodes, out ArgumentSyntax nodeToTest, bool inReturn = false)
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var solution = new AdhocWorkspace().CurrentSolution
                    .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                    .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddDocument(documentId, "MyFile.cs", documentCode);

            document = solution.GetDocument(documentId);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(documentCode);

            var compilation = CSharpCompilation.Create("HelloWorld")
                                   .AddReferences(
                                        MetadataReference.CreateFromFile(
                                            typeof(object).Assembly.Location))
                                   .AddSyntaxTrees(tree);

            semanticModel = compilation.GetSemanticModel(tree);        

            var treeRoot = (CompilationUnitSyntax)tree.GetRoot();
            nodes = treeRoot.DescendantNodesAndSelf();

            var sourceText = treeRoot.GetText();
            var line = sourceText.Lines[inReturn?21:11];
            var span = line.Span;
            var nodesAtLine = treeRoot.DescendantNodes(span);

            nodeToTest = nodesAtLine.OfType<ArgumentSyntax>().FirstOrDefault();

        }

        private ExpressionResult Execute(string insideCode, string insideClassCode, string outsideClassCode, string inReturnUnknowMethodExpression = "", bool inReturn = false)
        {
            var code = GenerateDocumentCodeFromTemplateUsingVariables(insideCode, insideClassCode, outsideClassCode, inReturnUnknowMethodExpression);
            Trace.WriteLine(code);
            PrepareAnalyzeModel(code, out var document, out var semanticModel, out var nodes, out var nodeToTest, inReturn);
            return new ExpressionResolver(150).GetStringValueOfExpression(document, nodeToTest?.Expression, nodes, semanticModel);
        }

        [Fact]
        public void NullDirect()
        {
            string insideMethodCode = @"null";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullInsideInterpolation()
        {
            string insideMethodCode = @"$""{null}""";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullInsideStringFormat()
        {
            string insideMethodCode = @"string.Format(""{0}"",null)";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullInAddExpression()
        {
            string insideMethodCode = @"null+""test""+null";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullInIdentifier()
        {
            string insideMethodCode = @"identifier";
            string insideClassCode = @"var identifier = null";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullInLambda()
        {
            string insideMethodCode = @"identifier";
            string insideClassCode = @"";
            string outsideClassCode = @"public string identifier => null";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullInGetter()
        {
            string insideMethodCode = @"identifier";
            string insideClassCode = @"";
            string outsideClassCode = @" public string identifier {
                            get
                            {
                                return null;
                            }
                            set
                            {
                            }
                        }";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void TerminatedBySemicolon()
        {
            string insideMethodCode = @";";
            string insideClassCode = @"";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void TerminatedByCurlyBracket()
        {
            string insideMethodCode = @"}";
            string insideClassCode = @"";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void WrongTypeInsideMethod()
        {
            string insideMethodCode = @"variable";
            string insideClassCode = @"int variable = 1;";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        //Even tho code is unbuildable, the syntax check should still be able to deliver when
        //analyzed portion of code is fine.
        [Fact]
        public void BadImplicityConvertion()
        {
            string insideMethodCode = @"""test""";
            string insideClassCode = @"int variable = ""xD"";";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("test", result.TextResult);
        }

        //If this case is crashing test suite etc. than assume that Assertion failed,
        //no matter of result in window.
        [Fact]
        public void CircularAssigment()
        {
            string insideMethodCode = @"var1";
            string insideClassCode = @"string var1 = var2;
                                       string var2 = var1;";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void Call5Times()
        {
            int i;
            var sb = new StringBuilder();
            sb.AppendLine(@"var c1 = ""finally here"";");
            for (i = 1; i < 5; i++)
            {
                sb.AppendLine($"var c{i + 1} = c{i};");
            }
            string insideMethodCode = $"c{i.ToString()}";
            string insideClassCode = sb.ToString();
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("finally here", result.TextResult);
        }

        //When calls extends ~1000(+-2) than method should return fallback value of ""
        [Fact]
        public void Call1001Times()
        {
            int i;
            var sb = new StringBuilder();
            sb.AppendLine(@"var c1 = ""finally here"";");
            for (i = 1; i < 1001; i++)
            {
                sb.AppendLine($"var c{i + 1} = c{i};");
            }
            string insideMethodCode = $"c{i.ToString()}";
            string insideClassCode = sb.ToString();
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

       [Fact]
        public void UnassignedVariable()
        {
            string insideMethodCode = @"variable";
            string insideClassCode = @"string variable;";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.Equal("", result.TextResult);
        }

        [Fact]
        public void NullWhenCallingPropertyIsUnassigned()
        {
            string insideMethodCode = @"";
            string insideClassCode = @"";
            string outsideClassCode = @"private readonly string variable;";
            string inReturnUnknowMethodExpression = @"$""{variable}.test""";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode, inReturnUnknowMethodExpression, true);
            Assert.Equal(".test", result.TextResult);
        }

        [Fact]
        public void StringReplaceOnNullString()
        {
            string insideMethodCode = @"nullString.Replace("""","""")";
            string insideClassCode = @"string nullString;";
            string outsideClassCode = @"";
            string inReturnUnknowMethodExpression = @"$""""";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode, inReturnUnknowMethodExpression);
            Assert.Equal(null, result.TextResult);
        }
    }
}
