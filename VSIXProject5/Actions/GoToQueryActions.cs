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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Actions.Abstracts;
using VSIXProject5.Actions.Shared;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.HelpersAndExtensions.VisualStudio;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.Parsers;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;
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
  
        public GoToQueryActions(GotoPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
        {
            base.package = package;
            _textManager = package.TextManager;
             _editorAdaptersFactory = package.EditorAdaptersFactory;
            _resultWindow = package.ResultWindow;
            _envDTE = package.EnvDTE;
            _statusBar = new StatusBarIntegration(package.IStatusBar);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            IVsTextView textView = null;
            _textManager.GetActiveView(1, null, out textView);
            textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);
            ITextSnapshot snapshot = wpfTextView.Caret.Position.BufferPosition.Snapshot;

            List<BaseIndexerValue> statments = null;
            ILineOperation lineOperation = snapshot.IsCSharpType() ? new CodeLineOperations() : (ILineOperation)(new XmlLineOperations());

            var queryName = lineOperation.GetQueryNameAtLine(snapshot, selectionLineNum);

            if (snapshot.GetContentTypeName() == "XML")
            {
                var statmentsKeys = Indexer.GetCodeKeysByQueryId(queryName);
                statments = statmentsKeys.Select(Indexer.GetCodeStatments).SelectMany(x => x).Cast<BaseIndexerValue>().ToList();

                if(statments.Count == 1)
                {
                    DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statments.First().QueryFilePath, statments.First().QueryLineNumber);
                }
                else if (!statments.Any())
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in Code.");
                }
                else
                {
                    _statusBar.ShowText($"Multiple occurence of same statment({queryName}) found.");
                }
            }
            else
            { 
                var keys = Indexer.GetXmlKeysByQueryId(queryName);
                if (keys.Any())
                {
                    var statment = Indexer.GetXmlStatmentOrNull(keys.First());
                    DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statment.QueryFilePath, statment.QueryLineNumber);
                }
                else
                {
                    statments = keys.Select(Indexer.GetXmlStatment).Cast<BaseIndexerValue>().ToList();
                    _statusBar.ShowText($"No occurence of query named: {queryName} found in SqlMaps.");
                }
            }
            if(statments != null && statments.Count > 1)
            {
                var windowContent = (ResultWindowControl)_resultWindow.Content;
                windowContent.ShowResults(statments);
                IVsWindowFrame windowFrame = (IVsWindowFrame)_resultWindow.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }
    }
}
