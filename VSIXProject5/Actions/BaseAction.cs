using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using VSIXProject5.Actions2.DocumentProcessors;
using VSIXProject5.Actions2.FinalActions;
using VSIXProject5.Actions2.TextProviders;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions2
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
        internal IFinalAction _finalAction;
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
            if (contentType == "CSharp")
            {
                _documentProcessor = await new CSharpDocumentProcessor(_documentPropertiesProvider.GetDocumentRepresentation(), _documentPropertiesProvider.GetSelectionLineNumber()).InitializeAsync();
                _finalAction = new CSharpDefaultFinalActions(_statusBar, _resultWindow);
            }
            if (contentType == "XML")
            {
                _documentProcessor = new XmlDocumentProcessor(_documentPropertiesProvider.GetDocumentRepresentation(), _documentPropertiesProvider.GetSelectionLineNumber()).Initialize();
                _finalAction = new DefaultFinalActions(_statusBar, _resultWindow);
            }

            var validationResult = _documentProcessor.GetValidator().CanJumpToQueryInLine(_documentPropertiesProvider.GetSelectionLineNumber());

            menuCommand.Visible = validationResult;
            menuCommand.Enabled = validationResult;          
        }
        public virtual void Change(object sender, EventArgs e)
        {

        }
    }
}
