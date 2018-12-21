using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.VSIntegration;

namespace VSIXProject5.Actions2
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
            var queryResult = _documentProcessor.TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue);
            _finalAction.PrepareAndExecuteGoToQuery(queryValue, expressionResult);
        }
    }
}
