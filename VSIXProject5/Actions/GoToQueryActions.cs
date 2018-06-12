using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Actions.Abstracts;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions.VisualStudio;
using VSIXProject5.Indexers;
using VSIXProject5.VSIntegration;
using static VSIXProject5.HelpersAndExtensions.XmlHelper;

namespace VSIXProject5.Actions
{
    public class GoToQueryActions : BaseActions
    {
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;
        private ToolWindowPane _resultWindow;
        private DTE2 _envDTE;

        public GoToQueryActions(GotoPackage package) 
        {
            base.package = package;
             _textManager = package.TextManager;
             _editorAdaptersFactory = package.EditorAdaptersFactory;
            _statusBar = new StatusBarIntegration(package.IStatusBar);
            _resultWindow = package.ResultWindow;
            _envDTE = package.EnvDTE;
        }

        public override void BeforeQuery(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            string activeDocumentLanguage = _envDTE.ActiveDocument.Language;
            if (activeDocumentLanguage == "CSharp")
            {
                //Get selection line number
                _textManager.GetActiveView(1, null, out IVsTextView textView);
                textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
                //Get carret position
                var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);
                SnapshotPoint caretPosition = wpfTextView.Caret.Position.BufferPosition;

                Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                if (doc == null)
                {
                    menuCommand.Visible = true;
                    menuCommand.Text = "Something went wrong";
                    return;
                }
                //Cool case: there is doc.TryGetSemanticModel but if it's not ready it will be null.
                //GetSemanticModelAsync will create it if needed, but not sure if using async event is good.
                SemanticModel semModel = doc.GetSemanticModelAsync().Result;

                NodeHelpers helper = new NodeHelpers(semModel);
                doc.TryGetSyntaxTree(out SyntaxTree synTree);

                var lineSpan = synTree.GetText().Lines[selectionLineNum].Span;
                var treeRoot = (CompilationUnitSyntax)synTree.GetRoot();
                var nodesAtLine = treeRoot.DescendantNodesAndSelf(lineSpan);

                var returnNode = helper.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
                if (returnNode != null)
                {
                    nodesAtLine = returnNode.DescendantNodesAndSelf();
                }
                menuCommand.Visible = helper.IsAnySyntaxNodeContainIBatisNamespace(nodesAtLine);
                menuCommand.Text = "Go to Query";
            }
            else if (activeDocumentLanguage == "XML")
            {
                EnvDTE.TextDocument doc = (EnvDTE.TextDocument)_envDTE.ActiveDocument.Object("TextDocument");

                TextSelection sel = (TextSelection)_envDTE.ActiveDocument.Selection;
                var lineText = sel.GetText();

                if (!XmlStringLine.IsIgnored(lineText))
                {
                    string text = doc.GetText();
                    var xDoc = XDocument.Parse(text);

                    bool isIBatisQueryXmlFile = XDocHelper.GetXDocumentNamespace(xDoc) == @"http://ibatis.apache.org/mapping";
                    if (isIBatisQueryXmlFile)
                    {
                        menuCommand.Visible = true;
                        menuCommand.Text = "Go to Query execution";
                    }
                }
            }
        }

        public override void Change(object sender, EventArgs e)
        {
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            bool isXmlFile = _envDTE.ActiveDocument.Language == "XML";

            IVsTextView textView = null;
            _textManager.GetActiveView(1, null, out textView);
            int selectionLineNum;
            textView.GetCaretPos(out selectionLineNum, out int selectionCol);
            if (isXmlFile)
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
                string queryName = "";
                if (idAtribute == null)
                    return;

                queryName = idAtribute.Value;

                var statmentsKeys = Indexer.GetCodeKeysByQueryId(queryName);

                var windowContent = (ResultWindowControl)_resultWindow.Content;
                var statmentsToShow = statmentsKeys.Select(x => Indexer.GetCodeStatments(x)).ToList().SelectMany(x => x).ToList();
                windowContent.ShowResults(statmentsToShow);
                IVsWindowFrame windowFrame = (IVsWindowFrame)_resultWindow.Frame;

                if (statmentsKeys.Count > 1)
                {
                    _statusBar.ShowText($"Multiple occurence of same statment({queryName}) in solution.");
                    ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    return;
                }
                var statmentKey = statmentsKeys.FirstOrDefault();
                if (statmentKey != null)
                {
                    var statments = Indexer.GetCodeStatmentOrNull(statmentKey);
                    if (statments.Count == 1)
                    {
                        var statment = statments.FirstOrDefault();
                        _envDTE.ItemOperations.OpenFile(statment.QueryFilePath);
                        TextSelection sel = (TextSelection)_envDTE.ActiveDocument.Selection;
                        TextPoint pnt = sel.ActivePoint;
                        sel.GotoLine(statment.QueryLineNumber, true);
                    }
                    else if (statments.Count > 1)
                    {
                        _statusBar.ShowText("Multiple occurence of same statment in project.");
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                        return;
                    }
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in Code.");
                }
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
                string queryName = "";
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
                var keys = Indexer.GetXmlKeysByQueryId(queryName);
                if (keys.Any())
                {
                    var windowContent = (ResultWindowControl)_resultWindow.Content;
                    var statmentsToShow = keys.Select(x => Indexer.GetXmlStatment(x)).ToList();
                    windowContent.ShowResults(statmentsToShow);

                    IVsWindowFrame windowFrame = (IVsWindowFrame)_resultWindow.Frame;

                    if (keys.Count == 1)
                    {
                        var statment = Indexer.GetXmlStatmentOrNull(keys.First());
                        _envDTE.ItemOperations.OpenFile(statment.QueryFilePath);
                        TextSelection sel = (TextSelection)_envDTE.ActiveDocument.Selection;
                        TextPoint pnt = sel.ActivePoint;
                        sel.GotoLine(statment.QueryLineNumber, true);
                    }
                    //This should never happend without implementing logic for multiple xml statments of same name.
                    else
                    {
                        _statusBar.ShowText($"Multiple statments of name:{queryName}");
                        //ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    }
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in SqlMaps.");
                }
            }
        }
    }
}
