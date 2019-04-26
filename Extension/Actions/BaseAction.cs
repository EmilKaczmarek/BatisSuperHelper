using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using IBatisSuperHelper.Actions.DocumentProcessors;
using IBatisSuperHelper.Actions.DocumentProcessors.Factory;
using IBatisSuperHelper.Actions.FinalActions;
using IBatisSuperHelper.Actions.FinalActions.Factory;
using IBatisSuperHelper.Actions.TextProviders;
using IBatisSuperHelper.VSIntegration;
using NLog;
using StackExchange.Profiling;
using IBatisSuperHelper.Logging.MiniProfiler;

namespace IBatisSuperHelper.Actions
{
    public abstract class BaseActions
    {
        internal GotoAsyncPackage Package { get; set; }
        public abstract void MenuItemCallback(object sender, EventArgs e);

        private readonly IVsTextManager _textManager;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        internal StatusBarIntegration StatusBar;
        internal IDocumentPropertiesProvider _documentPropertiesProvider;
        internal IDocumentProcessor _documentProcessor;
        internal IFinalActionFactory _finalActionFactory;

        internal ToolWindowPane _commandWindow => _commandWindowLazy?.Value;

        private readonly Lazy<ToolWindowPane> _commandWindowLazy;

        protected BaseActions(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdapersFactory, StatusBarIntegration statusBar, ToolWindowPane commandWindow)
            : this(textManager, editorAdapersFactory, statusBar)
        {
            _commandWindowLazy = new Lazy<ToolWindowPane>(() => commandWindow);
        }

        protected BaseActions(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdapersFactory, StatusBarIntegration statusBar)
        {
            _textManager = textManager;
            _editorAdaptersFactory = editorAdapersFactory;
            StatusBar = statusBar;
        }

        public virtual void BeforeQuery(object sender, EventArgs e)
        {
            var profiler = MiniProfiler.StartNew(nameof(BeforeQuery));
            profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
            using (profiler.Step("Event start"))
            {
                _documentPropertiesProvider = new TextManagerPropertiesProvider(_textManager, _editorAdaptersFactory);
                string contentType;
                using (profiler.Step("GetContentType"))
                {
                    contentType = _documentPropertiesProvider.GetContentType();
                }
                    
                DocumentProcessorFactory processorFactory;

                switch (contentType)
                {
                    case "CSharp":
                        processorFactory = new CSharpDocumentProcessorFactory();
                        _finalActionFactory = new CSharpFinalActionFactory();
                        
                        break;
                    case "XML":
                        processorFactory = new XmlDocumentProcessorFactory();
                        _finalActionFactory = new XmlFinalActionFactory();
                        break;
                    default:
                        return;
                }

                _documentProcessor = ThreadHelper.JoinableTaskFactory.Run(async () => await processorFactory.GetProcessorAsync(
                      _documentPropertiesProvider.GetDocumentRepresentation(),
                      _documentPropertiesProvider.GetSelectionLineNumber()
                      ));
            }
            profiler.Stop();
        }
        public virtual void Change(object sender, EventArgs e)
        {

        }
    }
}
