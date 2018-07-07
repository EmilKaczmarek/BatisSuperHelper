using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Linq;
using VSIXProject5.Actions.Abstracts;
using VSIXProject5.Actions.Shared;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions.VisualStudio;
using VSIXProject5.Indexers;
using VSIXProject5.VSIntegration;
using VSIXProject5.Windows.RenameWindow;
using VSIXProject5.Windows.RenameWindow.ViewModel;

namespace VSIXProject5.Actions
{
    public class QueryRenameActions : BaseActions
    {
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;
        private DTE2 _envDTE;
        private Workspace _workspace;

        public QueryRenameActions(GotoPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
        {
            base.package = package;
            _textManager = package.TextManager;
            _editorAdaptersFactory = package.EditorAdaptersFactory;
            _statusBar = new StatusBarIntegration(package.IStatusBar);
            _envDTE = package.EnvDTE;
            _workspace = package.Workspace;
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            IVsTextView textView = null;
            _textManager.GetActiveView(1, null, out textView);
            textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);

            ITextSnapshot snapshot = wpfTextView.Caret.Position.BufferPosition.Snapshot;
            ILineOperation lineOperation = snapshot.IsCSharpType() ? new CodeLineOperations() : (ILineOperation)(new XmlLineOperations());

            string queryName = lineOperation.GetQueryNameAtLine(snapshot, selectionLineNum);

            if (queryName == null)
            {
                return;
            }

            RenameModalWindowControl window = new RenameModalWindowControl(
                new RenameViewModel
                {
                    QueryText = queryName,
                });

            window.ShowModal();
            var returnViewModel = window.DataContext as RenameViewModel;

            if (returnViewModel.WasInputCanceled || returnViewModel.QueryText == queryName)
            {
                return;
            }

            var codeKeys = Indexer.Instance.GetCodeKeysByQueryId(queryName);
            var xmlKeys = Indexer.Instance.GetXmlKeysByQueryId(queryName);

            foreach (var xmlQuery in xmlKeys)
            {
                var query = Indexer.Instance.GetXmlStatment(xmlQuery);

                var projectItem = _envDTE.Solution.FindProjectItem(query.QueryFileName);
                var isProjectItemOpened = projectItem.IsOpen;
                if (!isProjectItemOpened)
                {
                    projectItem.Open();
                }
                var projectItemSelection = projectItem.Document.Selection as EnvDTE.TextSelection;
                projectItemSelection.StartOfDocument();

                var findResult = projectItemSelection.FindPattern(xmlQuery.StatmentName, (int)(vsFindOptions.vsFindOptionsMatchWholeWord));
                var replaceResult = projectItemSelection.ReplacePattern(xmlQuery.StatmentName, returnViewModel.QueryText, (int)(vsFindOptions.vsFindOptionsMatchWholeWord));
                Indexer.Instance.RenameXmlQuery(xmlQuery, returnViewModel.QueryText);
            }
            foreach (var codeKey in codeKeys) { 
                var codeQueries = Indexer.Instance.GetCodeStatments(codeKey);
                var group = codeQueries.GroupBy(x => x.DocumentId, x => x);
                foreach(var file in group)
                {
                    var doc = _workspace.CurrentSolution.GetDocument(file.Key);
                    SemanticModel semModel = doc.GetSemanticModelAsync().Result;
                    foreach (var query in file)
                    {
                        NodeHelpers helper = new NodeHelpers(semModel);
                        SyntaxTree synTree = null;
                        doc.TryGetSyntaxTree(out synTree);
                        var span = synTree.GetText().Lines[query.QueryLineNumber - 1].Span;
                        var root = (CompilationUnitSyntax)synTree.GetRoot();
                        var nodes = root.DescendantNodesAndSelf(span);
                        var syntaxArguments = helper.GetArgumentListSyntaxFromSyntaxNodesWhereArgumentsAreNotEmpty(nodes);
                        var singleArgumentListSyntax = helper.GetProperArgumentSyntaxNode(syntaxArguments);
                        var queryArgument = helper.GetArgumentSyntaxOfStringType(singleArgumentListSyntax);
                        var newArgumentSyntax = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(returnViewModel.QueryText)));
                        var newRoot = root.ReplaceNode(queryArgument, newArgumentSyntax);
                        root = newRoot;
                        doc = doc.WithText(newRoot.GetText());
                        var sucess = _workspace.TryApplyChanges(doc.Project.Solution);
                    }
                }
                Indexer.Instance.RenameCodeQueries(codeKey, returnViewModel.QueryText);
            }
        }
    }
}
