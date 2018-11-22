using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using VSIXProject5.Actions.Abstracts;
using VSIXProject5.Actions.Shared;
using VSIXProject5.HelpersAndExtensions;
using VSIXProject5.HelpersAndExtensions.VisualStudio;
using VSIXProject5.Indexers;
using VSIXProject5.Indexers.Models;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;
using VSIXProject5.Windows.ResultWindow.ViewModel;

namespace VSIXProject5.Actions
{
    public class GoToQueryActions : BaseActions
    {
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;
        private ToolWindowPane _resultWindow;
        private DTE2 _envDTE;
  
        public GoToQueryActions(GotoAsyncPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
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

            List<ResultWindowViewModel> windowViewModels = new List<ResultWindowViewModel>();
            ILineOperation lineOperation = snapshot.IsCSharpType()
                 ? new CodeLineOperations(snapshot, selectionLineNum)
                 : (ILineOperation)(new XmlLineOperations(snapshot, selectionLineNum));

            var queryName = lineOperation.GetQueryNameAtLine();

            if (snapshot.GetContentTypeName() == "XML")
            {
                var statmentsKeys = Indexer.Instance.GetCodeKeysByQueryId(queryName);
                var statments = statmentsKeys.Select(Indexer.Instance.GetCodeStatments).SelectMany(x => x);

                if (!statments.Any())
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in Code.");
                }
                if (statments.Count() == 1)
                {
                    DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statments.First().QueryFilePath, statments.First().QueryLineNumber);
                }
                if(statments.Count() > 1)
                {
                    windowViewModels = statments.Select(x => new ResultWindowViewModel
                    {
                        File = x.QueryFileName,
                        Line = x.QueryLineNumber,
                        Query = x.QueryId,
                        FilePath = x.QueryFilePath,
                        Namespace = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(x.QueryId).Item1,
                    }).ToList();
                    _statusBar.ShowText($"Multiple occurence of same statment({queryName}) found.");
                }
            }
            else
            { 
                var keys = Indexer.Instance.GetXmlKeysByQueryId(queryName);
                if (keys.Any())
                {
                    var statment = Indexer.Instance.GetXmlStatmentOrNull(keys.First());
                    windowViewModels = keys.Select(Indexer.Instance.GetXmlStatmentOrNull).Select(x => new ResultWindowViewModel
                    {
                        File = x.QueryFileName,
                        FilePath = x.QueryFilePath,
                        Line = x.QueryLineNumber,
                        Namespace = x.MapNamespace,
                        Query = x.QueryId,
                    }).ToList();
                    DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(statment.QueryFilePath, statment.QueryLineNumber);
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} found in SqlMaps.");
                }
            }

            if(windowViewModels != null && windowViewModels.Count > 1)
            {
                var windowContent = (ResultWindowControl)_resultWindow.Content;
                windowContent.ShowResults(windowViewModels);
                IVsWindowFrame windowFrame = (IVsWindowFrame)_resultWindow.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }
    }
}
