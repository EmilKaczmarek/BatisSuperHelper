using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VSIXProject5.Actions.Shared;
using VSIXProject5.Helpers;
using VSIXProject5.Indexers;
using VSIXProject5.Parsers;
using VSIXProject5.VSIntegration;
using static VSIXProject5.HelpersAndExtensions.XmlHelper;

namespace VSIXProject5.Actions.Abstracts
{
    public abstract class BaseActions
    {
        public GotoPackage package { get; set; }

        public abstract void MenuItemCallback(object sender, EventArgs e);

        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;

        internal BaseActions(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdapersFactory, StatusBarIntegration statusBar)
        {
            _textManager = textManager;
            _editorAdaptersFactory = editorAdapersFactory;
            _statusBar = statusBar;
        }
        public virtual void BeforeQuery(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            menuCommand.Visible = false;
            menuCommand.Enabled = false;

            _textManager.GetActiveView(1, null, out IVsTextView textView);
            textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
            //Get carret position
            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);
            SnapshotPoint caretPosition = wpfTextView.Caret.Position.BufferPosition;

            if (caretPosition.Snapshot.ContentType.TypeName == "CSharp")
            {
                Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

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
                var validLine = helper.IsAnySyntaxNodeContainIBatisNamespace(nodesAtLine);
                menuCommand.Visible = validLine;
                menuCommand.Enabled = validLine;
                menuCommand.Text = "Go to Query";
            }
            else if (caretPosition.Snapshot.ContentType.TypeName == "XML")
            {
                var lineText = caretPosition.GetContainingLine().GetText();
                if (!XmlStringLine.IsIgnored(lineText))
                {
                    string text = caretPosition.Snapshot.GetText();
                    XmlParser parser = XmlParser.WithStringReader(new StringReader(text));

                    bool isIBatisQueryXmlFile = parser.XmlNamespace == @"http://ibatis.apache.org/mapping";
                    bool isLineSupported = parser.HasSelectedLineValidQuery(selectionLineNum + 1);

                    if (isIBatisQueryXmlFile && isLineSupported)
                    {
                        menuCommand.Visible = true;
                        menuCommand.Enabled = true;
                        menuCommand.Text = "Go to Query execution";
                    }
                }
            }
        }
        public virtual void Change(object sender, EventArgs e)
        {

        }
    }
}
