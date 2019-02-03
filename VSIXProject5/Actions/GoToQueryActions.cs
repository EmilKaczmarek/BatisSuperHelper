using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using NLog;
using StackExchange.Profiling;
using System;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Loggers;
using VSIXProject5.Logging.MiniProfiler;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions
{
    public class GoToQueryActions2 : BaseActions
    {
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;
        private ToolWindowPane _resultWindow;
        private DTE2 _envDTE;
  
        public GoToQueryActions2(GotoAsyncPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
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
            try
            {
                var profiler = MiniProfiler.StartNew($"{nameof(GoToQueryActions2)}.{nameof(MenuItemCallback)}");
                profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
                using (profiler.Step("Event start"))
                {
                    var queryResult = _documentProcessor.TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue);
                    _finalEventActionsExecutor.Execute(queryValue, expressionResult);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "GoToQuery.MenuItemCallback");
                OutputWindowLogger.WriteLn($"Exception occured during GoToActionMenuCallback: {ex.Message}");
            }
          
        }
    }
}
