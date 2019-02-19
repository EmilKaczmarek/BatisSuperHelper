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
        private static Logger _log = LogManager.GetCurrentClassLogger();

        internal GotoAsyncPackage package { get; set; }
        public abstract void MenuItemCallback(object sender, EventArgs e);

        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        internal StatusBarIntegration _statusBar;
        internal IDocumentPropertiesProvider _documentPropertiesProvider;
        internal IDocumentProcessor _documentProcessor;
        internal IFinalActionFactory _finalActionFactory;

        internal ToolWindowPane _commandWindow => _commandWindowLazy?.Value;

        private Lazy<ToolWindowPane> _commandWindowLazy;

        internal BaseActions(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdapersFactory, StatusBarIntegration statusBar, ToolWindowPane commandWindow)
            : this(textManager, editorAdapersFactory, statusBar)
        {
            _commandWindowLazy = new Lazy<ToolWindowPane>(() => commandWindow);
        }

        internal BaseActions(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdapersFactory, StatusBarIntegration statusBar)
        {
            _textManager = textManager;
            _editorAdaptersFactory = editorAdapersFactory;
            _statusBar = statusBar;
        }

        public virtual async void BeforeQuery(object sender, EventArgs e)
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
                        //This is not mistake. When working on CSharp document, the result
                        //we are looking for is xml statment.
                        _finalActionFactory = new CSharpFinalActionFactory();
                        
                        break;
                    case "XML":
                        processorFactory = new XmlDocumentProcessorFactory();
                        //This is not mistake. When working on xml document, the result
                        //we are looking for is code statment.
                        _finalActionFactory = new XmlFinalActionFactory();
                        break;
                    default:
                        return;
                }

                processorFactory
                    .GetProcessorAsync(_documentPropertiesProvider.GetDocumentRepresentation(), _documentPropertiesProvider.GetSelectionLineNumber())
                    .ContinueWith((t) => _documentProcessor = t.Result).Wait();//Only fully working solution found that works for all processors...
            }
            profiler.Stop();
        }
        public virtual void Change(object sender, EventArgs e)
        {

        }
    }
}
