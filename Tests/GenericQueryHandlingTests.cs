using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Indexers.Code;
using Xunit;

namespace Tests
{
    public class GenericIndexingTests
    {
        private readonly string _directNamespaceText;
        private readonly string _fieldNamespaceText;
        private readonly string _fieldNamespaceCtor;

        private string GetString(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public GenericIndexingTests()
        {
            var asm = Assembly.GetExecutingAssembly();
            _directNamespaceText = GetString(asm.GetManifestResourceStream("Tests.Resources.DirectNamespace.txt"));
            _fieldNamespaceText = GetString(asm.GetManifestResourceStream("Tests.Resources.FieldNamespace.txt"));
            _fieldNamespaceCtor = GetString(asm.GetManifestResourceStream("Tests.Resources.FieldAssignedByCtor.txt"));

        }

        [Fact]
        public async Task SingleDirectNamespaceAsync()
        {
            ProjectId pid = ProjectId.CreateNewId();
            DocumentId did = DocumentId.CreateNewId(pid);
            var workspace = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(pid,"GenericTest","GenericTest", LanguageNames.CSharp)
                .AddMetadataReference(pid, 
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddDocument(did, 
                    "GenericDataAccess.cs", 
                    SourceText.From(_directNamespaceText));

            var genericDocument = workspace.GetDocument(did);
            var results = await new CSharpIndexer()
                .BuildFromDocumentAsync(genericDocument);

            Assert.NotNull(results);
            Assert.Single(results.Generics);
            Assert.Empty(results.Queries);

            var indexerResult = results.Generics.FirstOrDefault();

            Assert.NotNull(indexerResult);
            Assert.False(indexerResult.IsSolved);
            Assert.True(indexerResult.CanBeUsedAsQuery);
            Assert.Equal("T.SelectNumber", indexerResult.TextResult);
        }

        [Fact]
        public async Task SingleFieldNamespaceAsync()
        {
            ProjectId pid = ProjectId.CreateNewId();
            DocumentId did = DocumentId.CreateNewId(pid);
            var workspace = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(pid, "GenericTest", "GenericTest", LanguageNames.CSharp)
                .AddMetadataReference(pid,
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddDocument(did,
                    "GenericDataAccess.cs",
                    SourceText.From(_fieldNamespaceText));

            var genericDocument = workspace.GetDocument(did);
            var results = await new CSharpIndexer()
                .BuildFromDocumentAsync(genericDocument);

            Assert.NotNull(results);
            Assert.Single(results.Generics);
            Assert.Empty(results.Queries);

            var indexerResult = results.Generics.FirstOrDefault();

            Assert.NotNull(indexerResult);
            Assert.False(indexerResult.IsSolved);
            Assert.True(indexerResult.CanBeUsedAsQuery);
            Assert.Equal("T.SelectNumber", indexerResult.TextResult);
        }

        [Fact]
        public async Task SingleFieldAssignedByCtorNamespaceAsync()
        {
            ProjectId pid = ProjectId.CreateNewId();
            DocumentId did = DocumentId.CreateNewId(pid);
            var workspace = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(pid, "GenericTest", "GenericTest", LanguageNames.CSharp)
                .AddMetadataReference(pid,
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddDocument(did,
                    "GenericDataAccess.cs",
                    SourceText.From(_fieldNamespaceCtor));

            var genericDocument = workspace.GetDocument(did);
            var results = await new CSharpIndexer()
                .BuildFromDocumentAsync(genericDocument);

            Assert.NotNull(results);
            Assert.Single(results.Generics);
            Assert.Empty(results.Queries);

            var indexerResult = results.Generics.FirstOrDefault();

            Assert.NotNull(indexerResult);
            Assert.False(indexerResult.IsSolved);
            Assert.True(indexerResult.CanBeUsedAsQuery);
            Assert.Equal("T.SelectNumber", indexerResult.TextResult);
        }
    }
}
