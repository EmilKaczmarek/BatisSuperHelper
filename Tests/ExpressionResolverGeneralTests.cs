using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSIXProject5.HelpersAndExtensions.Roslyn;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolver;

namespace GoToQueryUnitTests
{
    [TestClass()]
    public class ExpressionResolverGeneralTests
    {
        private string _documentCSharpCode;
        private SourceText _sourceText;
        private CompilationUnitSyntax _treeRoot;
        private SemanticModel _semanticModel;
        private IEnumerable<SyntaxNode> _documentNodes;
        private Document _document;

        [TestInitialize]
        public void Initialize()
        {
            _documentCSharpCode =
                @"using System;
                using System.Collections;
                using System.Linq;
                using System.Text;
                 
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {                            
                            var case1 = TestMethod(""unittest"", """");
                            var case2 = TestMethod(""unit""+""test"","""");
                            var case3 = TestMethod(""unit""+""test"","""");
                            var case4 = TestMethod(""u""+""n""+""i""+""t""+""t""+""e""+""s""+""t"","""");
                            var case5 = TestMethod(constString,"""");
                            var case6 = TestMethod(constUnit+constTest,"""");
                            var case7 = TestMethod($""{constUnit}test"","""");
                            var case8 = TestMethod($""unit{constTest}"","""");
                            var case9 = TestMethod($""{constUnit}{constTest}"","""");
                            var case10 = TestMethod($""{""unit""}{""test""}"","""");
                            var case11 = TestMethod(stringFromInterpolation,"""");
                            var case12 = TestMethod(nameof(unittest),"""");
                            var case13 = TestMethod(constantStringUnitTest,"""");
                            var case14 = TestMethod(constantNameOfResult,"""");
                            var case15 = TestMethod(string.Format(""{0}"", ""unittest""),"""");                        
                            var case16 = TestMethod(string.Format(""{0}{1}"", ""unit"",""test""),"""");
                            var case17 = TestMethod(string.Format(""{0}{1}"", ""unit"", constTest),"""");
                            var case18 = TestMethod(string.Format(""{0}{1}"", constUnit, constTest),"""");
                            var case19 = TestMethod(typeof(unittest).Name,"""");
                            var case20 = TestMethod(constantTypeOfNameResult,"""");
                            var case21 = TestMethod(PublicFieldStringUnitTest,"""");
                            var case22 = TestMethod(PrivateFieldStringUnitTest,"""");
                            var case23 = TestMethod(PublicFieldStringUnitTestFromLambda,"""");
                            var case24 = TestMethod(PrivateFieldStringUnitTestFromLambda,"""");
                            var case25 = TestMethod(PublicFieldStringUnitTestWithGetter,"""");
                            var case26 = TestMethod(PrivateFieldStringUnitTestWithGetter,"""");
                            var case27 = TestMethod(PublicFieldStringUnitTestWithGetterNonDirectReturn,"""");
                            var case28 = TestMethod(PrivateFieldStringUnitTestWithGetterNonDirectReturn,"""");

                            var constString = ""unittest"";
                            var constUnit = ""unit"";
                            var constTest = ""test"";
                            const string constantStringUnitTest = ""unittest"";
                            const string constantNameOfResult = nameof(unittest);
                            var stringFromInterpolation = $""{constUnit}{constTest}"";
                            var stringFromStringFormat = string.Format(""{0}"", ""unittest"");
                            var constantTypeOfNameResult = typeof(unittest).Name;
                        }

                        public static string TestMethod(string one, string two)
                        {
                            return one + two;
                        }

                        public string PublicFieldStringUnitTest = ""unittest"";
                        private string PrivateFieldStringUnitTest = ""unittest"";
                        public string PublicFieldStringUnitTestFromLambda => ""unittest"";
                        private string PrivateFieldStringUnitTestFromLambda => ""unittest"";
                        public string PublicFieldStringUnitTestWithGetter {
                            get
                            {
                                return ""unittest"";
                            }
                            set
                            {
                            }
                        }
                        private string PrivateFieldStringUnitTestWithGetter {
                            get
                            {
                                return ""unittest"";
                            }
                            set
                            {
                            }
                        }
                        public string PublicFieldStringUnitTestWithGetterNonDirectReturn {
                            get
                            {
                                var returnValue =""unittest"";
                                return returnValue;
                            }
                            set
                            {
                            }
                        }
                        private string PrivateFieldStringUnitTestWithGetterNonDirectReturn {
                            get
                            {
                                var returnValue =""unittest"";
                                return returnValue;
                            }
                            set
                            {
                            }
                        }
                        public void unittest(){}
                    }
                }";
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var solution = new AdhocWorkspace().CurrentSolution
                    .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                    .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddDocument(documentId, "MyFile.cs", _documentCSharpCode);

            _document = solution.GetDocument(documentId);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(_documentCSharpCode);

            var compilation = CSharpCompilation.Create("HelloWorld")
                                   .AddReferences(
                                        MetadataReference.CreateFromFile(
                                            typeof(object).Assembly.Location))
                                   .AddSyntaxTrees(tree);

            _semanticModel = compilation.GetSemanticModel(tree);

            _treeRoot = (CompilationUnitSyntax)tree.GetRoot();
            _documentNodes = _treeRoot.DescendantNodesAndSelf();

            _sourceText = _treeRoot.GetText();

        }
        private ArgumentSyntax GetNodeFromSource(int lineNumber)
        {
            var line = _sourceText.Lines[lineNumber];
            var span = line.Span;
            var nodesAtLine = _treeRoot.DescendantNodes(span);

            return nodesAtLine.OfType<ArgumentSyntax>().FirstOrDefault();
        }

        [TestMethod]
        public void C01LiteralString()
        {
            var testNode = GetNodeFromSource(11);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C02AddExpressionWithDotAtSecoundExpression()
        {
            var testNode = GetNodeFromSource(12);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C03AddExpressionWithDotAtFirstExpression()
        {
            var testNode = GetNodeFromSource(13);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C04AddExpressionWithMultipleAddExpressionsNested()
        {
            var testNode = GetNodeFromSource(14);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C05StringIdentifier()
        {
            var testNode = GetNodeFromSource(15);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C06AddExpressionThatContainsStringIdentifiers()
        {
            var testNode = GetNodeFromSource(16);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C07InterpolationWithLeftLiteralAndRightIdentifier()
        {
            var testNode = GetNodeFromSource(17);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C08InterpolationWithLeftIdentifierAndRightLiteral()
        {
            var testNode = GetNodeFromSource(18);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C09InterpolationWithOnlyIdentifier()
        {
            var testNode = GetNodeFromSource(19);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C10InterpolationWithOnlyLiterals()
        {
            var testNode = GetNodeFromSource(20);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C11LiteralConstantNameOfExpression()
        {
            var testNode = GetNodeFromSource(21);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C12IndentifierWithInterpolation()
        {
            var testNode = GetNodeFromSource(22);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C13ConstantIdentifierWithLiteralExpression()
        {
            var testNode = GetNodeFromSource(23);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C14ConstantIdentifierWithNameOfExpression()
        {
            var testNode = GetNodeFromSource(24);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C15LiteralStringFormat()
        {
            var testNode = GetNodeFromSource(25);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C16LiteralStringFormatWithTwoLiterals()
        {
            var testNode = GetNodeFromSource(26);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C17LiteralStringFormatWithOneLiteralAndOneIdentifier()
        {
            var testNode = GetNodeFromSource(27);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C18LiteralStringFormatWithTwoIdentifiers()
        {
            var testNode = GetNodeFromSource(28);
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C19LiteralConstantTypeOfNameExpression()
        {
            var testNode = GetNodeFromSource(29);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C20ConstantIdentifierWithTypeOfNameExpression()
        {
            var testNode = GetNodeFromSource(30);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C21PublicIndentifierOutsideMethod()
        {
            var testNode = GetNodeFromSource(31);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C22PrivateIndentifierOutsideMethod()
        {
            var testNode = GetNodeFromSource(32);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C23PublicIndentifierWithLambdaOutsideMethod()
        {
            var testNode = GetNodeFromSource(33);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C24PrivateIndentifierWithLambdaOutsideMethod()
        {
            var testNode = GetNodeFromSource(34);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C25PublicIndentifierWithGetterOutsideMethod()
        {
            var testNode = GetNodeFromSource(35);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C26PrivateIndentifierWithGetterOutsideMethod()
        {
            var testNode = GetNodeFromSource(36);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C27PublicIndentifierWithGetterOutsideMethodNonDirectReturn()
        {
            var testNode = GetNodeFromSource(37);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }

        [TestMethod]
        public void C28PrivateIndentifierWithGetterOutsideMethodNonDirectReturn()
        {
            var testNode = GetNodeFromSource(38);
            Trace.WriteLine(testNode.GetText());
            var result = new ExpressionResolver().GetStringValueOfExpression(_document, testNode.Expression, _documentNodes, _semanticModel);
            Assert.AreNotEqual("", result.TextResult);
            Assert.AreEqual("unittest", result.TextResult);
        }
    }
}
