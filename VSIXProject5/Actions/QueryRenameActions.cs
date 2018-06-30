using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Actions.Abstracts;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions.VisualStudio;
using VSIXProject5.Indexers;
using VSIXProject5.VSIntegration;
using VSIXProject5.Windows.RenameWindow;
using VSIXProject5.Windows.RenameWindow.ViewModel;
using static VSIXProject5.HelpersAndExtensions.XmlHelper;

namespace VSIXProject5.Actions
{
    public class QueryRenameActions : BaseStatementAtLineActions
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
        public override void BeforeQuery(object sender, EventArgs e)
        {
            base.BeforeQuery(sender, e);
        }

        public override void Change(object sender, EventArgs e)
        {
            base.Change(sender, e);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            IVsTextView textView = null;
            _textManager.GetActiveView(1, null, out textView);
            textView.GetCaretPos(out int selectionLineNum, out int selectionCol);

            string queryName = null;
            if(_envDTE.ActiveDocument.Language == "XML")
            {
                EnvDTE.TextDocument doc = (EnvDTE.TextDocument)(_envDTE.ActiveDocument.Object("TextDocument"));
                string xmlTextContent = doc.GetText();

                var xmlDocument = XDocument.Parse(xmlTextContent, LoadOptions.SetLineInfo);
                var xElements = xmlDocument.Descendants();
                var elementsLineNumbers = xElements.Select(x => ((IXmlLineInfo)x).LineNumber).ToList();
                int lineNumber = selectionLineNum + 1;//Missmatch between visual studio lines numeration and text lines numeration
                int? elementLocation = elementsLineNumbers.Cast<int?>().FirstOrDefault(x => x == lineNumber);

                if (elementLocation == null)
                {
                    elementsLineNumbers.Add(lineNumber);
                    elementsLineNumbers.Sort();
                    int indexOfLineNumber = elementsLineNumbers.IndexOf(lineNumber);
                    elementLocation = elementsLineNumbers[indexOfLineNumber == 0 ? 0 : indexOfLineNumber - 1];
                }

                var node = xElements.FirstOrDefault(x => ((IXmlLineInfo)x).LineNumber == elementLocation);
                var idAtribute = node.Attributes().FirstOrDefault(x => x.Name.LocalName == IBatisConstants.StatmentIdAttributeName);
                if (idAtribute == null)
                    return;

                queryName = idAtribute.Value;
            }
            else
            {
                var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);
                SnapshotPoint caretPosition = wpfTextView.Caret.Position.BufferPosition;
                Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                SemanticModel semModel = doc.GetSemanticModelAsync().Result;

                NodeHelpers helper = new NodeHelpers(semModel);
                SyntaxTree synTree = null;
                doc.TryGetSyntaxTree(out synTree);
                var span = synTree.GetText().Lines[selectionLineNum].Span;
                var root = (CompilationUnitSyntax)synTree.GetRoot();
                var nodesAtLine = root.DescendantNodesAndSelf(span);

                var returnNode = helper.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
                //Check if current document line is having 'return' keyword.
                //In this case we need to Descendant Node to find ArgumentList
                if (returnNode != null)
                {
                    var ReturnNodeDescendanted = returnNode.DescendantNodesAndSelf();
                    queryName = helper.GetQueryStringFromSyntaxNodes(ReturnNodeDescendanted);
                }
                //In case we don't have cursor around 'return', SyntaxNodes taken from line
                //should have needed ArgumentLineSyntax
                else
                {
                    queryName = helper.GetQueryStringFromSyntaxNodes(nodesAtLine);
                }
            }

            if (queryName == null)
            {
                return;
            }

            RenameModalWindowControl window = new RenameModalWindowControl(
                new RenameViewModel
                {
                    QueryText = queryName,
                });

            var wtfbool = window.ShowModal();
            var returnViewModel = window.DataContext as RenameViewModel;
            if (returnViewModel.WasInputCanceled || returnViewModel.QueryText == queryName)
            {
                return;
            }
            var codeKeys = Indexer.GetCodeKeysByQueryId(queryName);
            var xmlKeys = Indexer.GetXmlKeysByQueryId(queryName);
            foreach (var xmlQuery in xmlKeys)
            {
                var query = Indexer.GetXmlStatment(xmlQuery);

                var projectItem = _envDTE.Solution.FindProjectItem(query.QueryFileName);
                var isProjectItemOpened = projectItem.IsOpen;
                if (!isProjectItemOpened)
                {
                    projectItem.Open();
                }
                var kurwa = projectItem.Properties;
                var wtfDocument = projectItem.Document;
                var projectItemSelection = projectItem.Document.Selection as EnvDTE.TextSelection;
                projectItemSelection.StartOfDocument();

                var wtf = projectItemSelection.FindPattern(xmlQuery.StatmentName, (int)(vsFindOptions.vsFindOptionsMatchWholeWord));
                var wtfBool = projectItemSelection.ReplacePattern(xmlQuery.StatmentName, returnViewModel.QueryText, (int)(vsFindOptions.vsFindOptionsMatchWholeWord));
                Indexer.RenameXmlQuery(xmlQuery, returnViewModel.QueryText);
            }
            foreach (var codeKey in codeKeys) { 
                var codeQueries = Indexer.GetCodeStatments(codeKey);
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
                Indexer.RenameCodeQueries(codeKey, returnViewModel.QueryText);
            }
        }
    }
}
