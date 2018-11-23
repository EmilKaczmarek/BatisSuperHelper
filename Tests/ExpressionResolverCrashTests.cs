using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSIXProject5.HelpersAndExtensions.Roslyn;

namespace GoToQueryUnitTests
{
    [TestClass]
    public class ExpressionResolverCrashTests
    {
        private string _codeTemplateP1;
        private string _codeTemplateP2;
        private string _codeTemplateP3;
        private string _codeTemplateP4;
        private string _codeTemplateP5;

        [AssemblyInitialize]
        public static void InitializeReferencedAssemblies(TestContext context)
        {
            //Uncomment this dummies if there are issues with loading assemblies.
            //using (var dummy = new Microsoft.CodeAnalysis.AdhocWorkspace())
            //{
            //}

            //var dummy2 = new Microsoft.CodeAnalysis.WorkspaceChangeKind();
            //var dummy3 = typeof(Microsoft.CodeAnalysis.CSharp.SyntaxFactory);
        }
        [TestInitialize]
        public void Initialize()
        {
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
        private void PrepareAnalyzeModel(string documentCode, out SemanticModel semanticModel,out IEnumerable<SyntaxNode> nodes, out ArgumentSyntax nodeToTest, bool inReturn = false)
        {
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
        private string Execute(string insideCode, string insideClassCode, string outsideClassCode, string inReturnUnknowMethodExpression = "", bool inReturn = false)
        {
            var code = GenerateDocumentCodeFromTemplateUsingVariables(insideCode, insideClassCode, outsideClassCode, inReturnUnknowMethodExpression);
            Trace.WriteLine(code);
            PrepareAnalyzeModel(code, out var semanticModel, out var nodes, out var nodeToTest, inReturn);
            return new ExpressionResolver().GetStringValueOfExpression(nodeToTest?.Expression, nodes, semanticModel);
        }

        public void TextGenerationExample()
        {
            string insideMethodCode = @"test";
            string insideClassCode = @"var test2= ""unittest""";
            string outsideClassCode = @"public string test = ""unittest""";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreNotEqual("", result);
            Assert.AreEqual("unittest", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullDirect()
        {
            string insideMethodCode = @"null";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullInsideInterpolation()
        {
            string insideMethodCode = @"$""{null}""";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullInsideStringFormat()
        {
            string insideMethodCode = @"string.Format(""{0}"",null)";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullInAddExpression()
        {
            string insideMethodCode = @"null+""test""+null";
            string insideClassCode = @"";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("test", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullInIdentifier()
        {
            string insideMethodCode = @"identifier";
            string insideClassCode = @"var identifier = null";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullInLambda()
        {
            string insideMethodCode = @"identifier";
            string insideClassCode = @"";
            string outsideClassCode = @"public string identifier => null";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Null Handling")]
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
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Unbuildable code")]
        public void TerminatedBySemicolon()
        {
            string insideMethodCode = @";";
            string insideClassCode = @"";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Unbuildable code")]
        public void TerminatedByCurlyBracket()
        {
            string insideMethodCode = @"}";
            string insideClassCode = @"";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Unbuildable code")]
        public void WrongTypeInsideMethod()
        {
            string insideMethodCode = @"variable";
            string insideClassCode = @"int variable = 1;";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        //Even tho code is unbuildable, the syntax check should still be able to deliver when
        //analyzed portion of code is fine.
        [TestMethod, TestCategory("Unbuildable code")]
        public void BadImplicityConvertion()
        {
            string insideMethodCode = @"""test""";
            string insideClassCode = @"int variable = ""xD"";";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("test", result);
        }

        //If this case is crashing test suite etc. than assume that Assertion failed,
        //no matter of result in window.
        [TestMethod, TestCategory("Stackoverflow prevention")]
        public void CircularAssigment()
        {
            string insideMethodCode = @"var1";
            string insideClassCode = @"string var1 = var2;
                                       string var2 = var1;";
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Stackoverflow prevention")]
        public void Call10Times()
        {
            int i;
            var sb = new StringBuilder();
            sb.AppendLine(@"var c1 = ""finally here"";");
            for (i = 1; i < 10; i++)
            {
                sb.AppendLine($"var c{i + 1} = c{i};");
            }
            string insideMethodCode = $"c{i.ToString()}";
            string insideClassCode = sb.ToString();
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("finally here", result);
        }

        [TestMethod, TestCategory("Stackoverflow prevention")]
        public void Call100Times()
        {
            int i;
            var sb = new StringBuilder();
            sb.AppendLine(@"var c1 = ""finally here"";");
            for (i = 1; i < 100; i++)
            {
                sb.AppendLine($"var c{i + 1} = c{i};");
            }
            string insideMethodCode = $"c{i.ToString()}";
            string insideClassCode = sb.ToString();
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("finally here", result);
        }

        [TestMethod, TestCategory("Stackoverflow prevention")]
        public void Call1000Times()
        {
            int i;
            var sb = new StringBuilder();
            sb.AppendLine(@"var c1 = ""finally here"";");
            for (i = 1; i < 1000; i++)
            {
                sb.AppendLine($"var c{i + 1} = c{i};");
            }
            string insideMethodCode = $"c{i.ToString()}";
            string insideClassCode = sb.ToString();
            string outsideClassCode = @"";

            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("finally here", result);
        }
        //When calls extends ~1000(+-2) than method should return fallback value of ""
        [TestMethod, TestCategory("Stackoverflow prevention")]
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
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("General/No category")]
        public void UnassignedVariable()
        {
            string insideMethodCode = @"variable";
            string insideClassCode = @"string variable;";
            string outsideClassCode = @"";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode);
            Assert.AreEqual("", result);
        }

        [TestMethod, TestCategory("Null Handling")]
        public void NullWhenCallingPropertyIsUnassigned()
        {
            string insideMethodCode = @"";
            string insideClassCode = @"";
            string outsideClassCode = @"private readonly string variable;";
            string inReturnUnknowMethodExpression = @"$""{variable}.test""";
            var result = Execute(insideMethodCode, insideClassCode, outsideClassCode, inReturnUnknowMethodExpression, true);
            Assert.AreEqual(".test", result);
        }
    }
}
