using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IBatisSuperHelper.Indexers;

namespace Tests
{
    [TestClass]
    public class GenericIndexingTests
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

        [TestInitialize]
        public void Initialize()
        {
            var asm = Assembly.GetExecutingAssembly();
            _directNamespaceText = GetString(asm.GetManifestResourceStream("Tests.Resources.DirectNamespace.txt"));
            _fieldNamespaceText = GetString(asm.GetManifestResourceStream("Tests.Resources.FieldNamespace.txt"));
            _fieldNamespaceCtor = GetString(asm.GetManifestResourceStream("Tests.Resources.FieldAssignedByCtor.txt"));

        }

        [TestMethod, ]
        public void SingleDirectNamespace()
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
            var results = new CSharpIndexer()
                .BuildFromDocumentAsync(genericDocument)
                .Result;

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Generics.Count, 1);
            Assert.AreEqual(results.Queries.Count, 0);

            var indexerResult = results.Generics.FirstOrDefault();

            Assert.IsNotNull(indexerResult);
            Assert.IsFalse(indexerResult.IsSolved);
            Assert.IsTrue(indexerResult.CanBeUsedAsQuery);
            Assert.AreEqual(indexerResult.TextResult, "T.SelectNumber");
        }

        [TestMethod]
        public void SingleFieldNamespace()
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
            var results = new CSharpIndexer()
                .BuildFromDocumentAsync(genericDocument)
                .Result;

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Generics.Count, 1);
            Assert.AreEqual(results.Queries.Count, 0);

            var indexerResult = results.Generics.FirstOrDefault();

            Assert.IsNotNull(indexerResult);
            Assert.IsFalse(indexerResult.IsSolved);
            Assert.IsTrue(indexerResult.CanBeUsedAsQuery);
            Assert.AreEqual(indexerResult.TextResult, "T.SelectNumber");
        }

        [TestMethod]
        public void SingleFieldAssignedByCtorNamespace()
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
            var results = new CSharpIndexer()
                .BuildFromDocumentAsync(genericDocument)
                .Result;

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Generics.Count, 1);
            Assert.AreEqual(results.Queries.Count, 0);

            var indexerResult = results.Generics.FirstOrDefault();

            Assert.IsNotNull(indexerResult);
            Assert.IsFalse(indexerResult.IsSolved);
            Assert.IsTrue(indexerResult.CanBeUsedAsQuery);
            Assert.AreEqual(indexerResult.TextResult, "T.SelectNumber");
        }
    }
}
