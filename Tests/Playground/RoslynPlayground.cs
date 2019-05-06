using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Xunit;

#pragma warning disable S1481 // Unused local variables should be removed
#pragma warning disable S125 // Sections of code should not be commented out

namespace Playground
{
    public class RoslynPlayground
    {
        private string _directNamespaceText;
        private string _fieldNamespaceText;
        private string _fieldNamespaceCtor;

        private string GetString(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public RoslynPlayground()
        {
            var asm = Assembly.GetExecutingAssembly();
            _directNamespaceText = GetString(asm.GetManifestResourceStream("Tests.Resources.DirectNamespace.txt"));
            _fieldNamespaceText = GetString(asm.GetManifestResourceStream("Tests.Resources.FieldNamespace.txt"));
            _fieldNamespaceCtor = GetString(asm.GetManifestResourceStream("Tests.Resources.FieldAssignedByCtor.txt"));

        }

        [Fact]
        public async Task SingleDirectNamespaceAsync()
        {
            var code = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericTest
{
    public class GenericTableCalls
    {

        public void CallGeneric()
        {
            var genericDA = new GenericDataAccess<SomeTable>();
            var result = genericDA.GetAll();
        }
    }
}
";
            ProjectId pid = ProjectId.CreateNewId();
            DocumentId did = DocumentId.CreateNewId(pid);
            DocumentId mainId = DocumentId.CreateNewId(pid);
            var workspace = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(pid, "GenericTest", "GenericTest", LanguageNames.CSharp)
                .AddMetadataReference(pid,
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddDocument(did,
                    "GenericDataAccess.cs",
                    SourceText.From(_fieldNamespaceCtor))
                 .AddDocument(mainId, "main.cs", SourceText.From(code));
                
            var document = workspace.GetDocument(did);

            var semanticModel = await document.GetSemanticModelAsync();

            var syntaxTree = await document.GetSyntaxTreeAsync();

            var text = await syntaxTree.GetTextAsync();

            var line = text.Lines[8];
            var span = line.Span;
            var nodesAtLine = (await syntaxTree.GetRootAsync()).DescendantNodes(span).OfType<VariableDeclarationSyntax>().FirstOrDefault();


            var test1 = nodesAtLine.Ancestors();
            var test2 = nodesAtLine.ChildNodes();

            var initializer = (await syntaxTree.GetRootAsync()).DescendantNodes(text.Lines[12].Span);


            List<List<ISymbol>> xx = new List<List<ISymbol>>();
            List<List<SymbolCallerInfo>> xxx = new List<List<SymbolCallerInfo>>();
            Dictionary<SyntaxNode, ControlFlowAnalysis> cfl = new Dictionary<SyntaxNode, ControlFlowAnalysis>();
            Dictionary<SyntaxNode, DataFlowAnalysis> dfl = new Dictionary<SyntaxNode, DataFlowAnalysis>();

            foreach (var node in initializer)
            {
                
                if (node is StatementSyntax)
                {
                    cfl.Add(node, semanticModel.AnalyzeControlFlow(node));
                    dfl.Add(node, semanticModel.AnalyzeDataFlow(node));
                }
                if (node is ExpressionSyntax)
                {
                    dfl.Add(node, semanticModel.AnalyzeDataFlow(node));
                }


                            //var symbol = semanticModel.GetDeclaredSymbol(node);
                            //if (symbol != null)
                            //{
                            //    xx.Add(SymbolFinder.FindImplementationsAsync(symbol, workspace).Result.ToList());
                            //    xxx.Add(SymbolFinder.FindCallersAsync(symbol, workspace).Result.ToList());
                            //    var t = SymbolFinder.
                            //}

            }

        }
    }
}
