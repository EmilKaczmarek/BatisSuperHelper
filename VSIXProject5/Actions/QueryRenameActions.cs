//using EnvDTE;
//using EnvDTE80;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.VisualStudio.Editor;
//using Microsoft.VisualStudio.Shell;
//using Microsoft.VisualStudio.Text;
//using Microsoft.VisualStudio.TextManager.Interop;
//using System;
//using System.Linq;
//using VSIXProject5.Actions.Abstracts;
//using VSIXProject5.Actions.Shared;
//using VSIXProject5.Helpers;
//using VSIXProject5.HelpersAndExtensions;
//using VSIXProject5.HelpersAndExtensions.VisualStudio;
//using VSIXProject5.Indexers;
//using VSIXProject5.Storage;
//using VSIXProject5.VSIntegration;
//using VSIXProject5.Windows.RenameWindow;
//using VSIXProject5.Windows.RenameWindow.ViewModel;

//namespace VSIXProject5.Actions
//{
//    public class QueryRenameActions : BaseActions
//    {
//        private IVsTextManager _textManager;
//        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
//        private StatusBarIntegration _statusBar;
//        private DTE2 _envDTE;
//        private Workspace _workspace;

//        public QueryRenameActions(GotoAsyncPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
//        {
//            base.package = package;
//            _textManager = package.TextManager;
//            _editorAdaptersFactory = package.EditorAdaptersFactory;
//            _statusBar = new StatusBarIntegration(package.IStatusBar);
//            _envDTE = package.EnvDTE;
//            _workspace = package.Workspace;
//        }

//        public override void BeforeQuery(object sender, EventArgs e)
//        {
//            base.BeforeQuery(sender, e);

//            IVsTextView textView = null;
//            _textManager.GetActiveView(1, null, out textView);
//            textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
//            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);

//            ITextSnapshot snapshot = wpfTextView.Caret.Position.BufferPosition.Snapshot;
//            ILineOperation lineOperation = snapshot.IsCSharpType()
//                ? new CodeLineOperations(snapshot, selectionLineNum)
//                : (ILineOperation)(new XmlLineOperations(snapshot, selectionLineNum));

//            OleMenuCommand menuCommand = sender as OleMenuCommand;
//            menuCommand.Enabled = lineOperation.CanRenameQueryAtLine();
//        }

//        public override void MenuItemCallback(object sender, EventArgs e)
//        {
//            IVsTextView textView = null;
//            _textManager.GetActiveView(1, null, out textView);
//            textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
//            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);

//            ITextSnapshot snapshot = wpfTextView.Caret.Position.BufferPosition.Snapshot;
//            ILineOperation lineOperation = snapshot.IsCSharpType() 
//                ? new CodeLineOperations(snapshot, selectionLineNum) 
//                : (ILineOperation)(new XmlLineOperations(snapshot, selectionLineNum));

//            string queryName = lineOperation.GetQueryNameAtLine();

//            if (queryName == null)
//            {
//                return;
//            }

//            var codeKeys = PackageStorage.CodeQueries.GetKeysByQueryId(queryName);
//            var xmlKeys = PackageStorage.XmlQueries.GetKeysByQueryId(queryName);

//            var namespaceQueryPair = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(queryName);

//            RenameModalWindowControl window = new RenameModalWindowControl(
//                new RenameViewModel
//                {
//                    QueryText = namespaceQueryPair.Item2,
//                    Namespace = string.IsNullOrEmpty(namespaceQueryPair.Item1)?null:namespaceQueryPair.Item1,
//                });

//            window.ShowModal();
//            var returnViewModel = window.DataContext as RenameViewModel;

//            if (returnViewModel.WasInputCanceled || returnViewModel.QueryText == queryName)
//            {
//                return;
//            }           

//            foreach (var xmlQuery in xmlKeys)
//            {
//                var query = PackageStorage.XmlQueries.GetValue(xmlQuery);
//                var projectItem = _envDTE.Solution.FindProjectItem(query.QueryFileName);

//                var isProjectItemOpened = projectItem.IsOpen;
//                if (!isProjectItemOpened)
//                {
//                    projectItem.Open();
//                }

//                var textSelection = projectItem.Document.Selection as TextSelection;
//                textSelection.GotoLine(query.QueryLineNumber, true);

//                var line = textSelection.GetText();
//                line = line.Replace(MapNamespaceHelper.GetQueryWithoutNamespace(query), MapNamespaceHelper.GetQueryWithoutNamespace(returnViewModel.QueryText));

//                textSelection.Insert(line, (int)vsInsertFlags.vsInsertFlagsContainNewText);
//                projectItem.Document.Save();      

//                PackageStorage.XmlQueries.RenameQuery(xmlQuery, returnViewModel.QueryText);
//            }
//            foreach (var codeKey in codeKeys) { 
//                var codeQueries = PackageStorage.CodeQueries.GetValue(codeKey);
//                foreach(var file in codeQueries.GroupBy(x => x.DocumentId, x => x))
//                {
//                    var doc = _workspace.CurrentSolution.GetDocument(file.Key);
//                    SemanticModel semModel = doc.GetSemanticModelAsync().Result;
//                    NodeHelpers helper = new NodeHelpers(semModel);
//                    doc.TryGetSyntaxTree(out SyntaxTree synTree);
//                    var root = (CompilationUnitSyntax)synTree.GetRoot();
//                    var newArgumentSyntax = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(returnViewModel.QueryText)));

//                    foreach (var query in file)
//                    {                    
//                        var span = synTree.GetText().Lines[query.QueryLineNumber - 1].Span;                     
//                        var nodes = root.DescendantNodesAndSelf(span);
//                        var existingQueryArgumentSyntax = helper.GetProperArgumentNodeInNodes(nodes);
                       
//                        var newRoot = root.ReplaceNode(existingQueryArgumentSyntax, newArgumentSyntax);
//                        root = newRoot;

//                        doc = doc.WithText(newRoot.GetText());
//                        var sucess = _workspace.TryApplyChanges(doc.Project.Solution);
//                    }
//                }
//                PackageStorage.CodeQueries.RenameQuery(codeKey, returnViewModel.QueryText);
//            }
//        }
//    }
//}
