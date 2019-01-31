using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using VSIXProject5.Actions.DocumentProcessors;
using VSIXProject5.Actions.DocumentProcessors.Factory;
using VSIXProject5.Actions.FinalActions;
using VSIXProject5.Actions.FinalActions.Factory;
using VSIXProject5.Actions.DocumentProcessors;
using VSIXProject5.Actions.TextProviders;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions
{
    public abstract class BaseActions
    {
        internal GotoAsyncPackage package { get; set; }
        public abstract void MenuItemCallback(object sender, EventArgs e);

        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;

        internal IDocumentPropertiesProvider _documentPropertiesProvider;
        internal IDocumentProcessor _documentProcessor;
        internal FinalEventActionsExecutor _finalEventActionsExecutor;

        internal ToolWindowPane _resultWindow => _resultWindowLazy.Value;

        private Lazy<ToolWindowPane> _resultWindowLazy;

        internal BaseActions(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdapersFactory, StatusBarIntegration statusBar)
        {
            _textManager = textManager;
            _editorAdaptersFactory = editorAdapersFactory;
            _statusBar = statusBar;
            _resultWindowLazy = new Lazy<ToolWindowPane>(()=>package.ResultWindow);
        }
        public virtual async void BeforeQuery(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            menuCommand.Text = "Go to Query";
            menuCommand.Visible = false;
            menuCommand.Enabled = false;

            _documentPropertiesProvider = new TextManagerPropertiesProvider(_textManager, _editorAdaptersFactory);

            var contentType = _documentPropertiesProvider.GetContentType();

            DocumentProcessorFactory processorFactory;
            IFinalActionFactory finalActionFactory;

            switch (contentType) {
                case "CSharp":
                    processorFactory = new CSharpDocumentProcessorFactory();
                    //This is not mistake. When working on CSharp document, the result
                    //we are looking for is xml statment.
                    finalActionFactory = new XmlFinalActionFactory();
                    break;
                case "XML":
                    processorFactory = new XmlDocumentProcessorFactory();
                    //This is not mistake. When working on xml document, the result
                    //we are looking for is code statment.
                    finalActionFactory = new CSharpFinalActionFactory();
                    break;
                default:
                    return;
            }

            processorFactory
                .GetProcessorAsync(_documentPropertiesProvider.GetDocumentRepresentation(), _documentPropertiesProvider.GetSelectionLineNumber())
                .ContinueWith((t) => _documentProcessor = t.Result).Wait();//Only fully working solution found that works for all processors...

            _finalEventActionsExecutor = finalActionFactory.GetFinalEventActionsExecutor(_statusBar, _resultWindow);

            var validator = _documentProcessor.GetValidator();
            var validationTask = validator.CanJumpToQueryInLine(_documentPropertiesProvider.GetSelectionLineNumber());

            menuCommand.Visible = validationTask;
            menuCommand.Enabled = validationTask;
        }
        public virtual void Change(object sender, EventArgs e)
        {

        }
    }
}
