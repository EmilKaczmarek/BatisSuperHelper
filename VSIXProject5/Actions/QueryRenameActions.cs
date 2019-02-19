using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Linq;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.HelpersAndExtensions.VisualStudio;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.VSIntegration;
using IBatisSuperHelper.Windows.RenameWindow;
using IBatisSuperHelper.Windows.RenameWindow.ViewModel;
using IBatisSuperHelper.Actions.TextProviders;
using StackExchange.Profiling;
using IBatisSuperHelper.Logging.MiniProfiler;
using NLog;
using IBatisSuperHelper.Actions.DocumentProcessors;
using IBatisSuperHelper.Actions.FinalActions.Factory;
using IBatisSuperHelper.Actions.DocumentProcessors.Factory;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace IBatisSuperHelper.Actions
{
    public class QueryRenameActions : BaseActions
    {
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private StatusBarIntegration _statusBar;
        private DTE2 _envDTE;
        private Workspace _workspace;

        public QueryRenameActions(GotoAsyncPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
        {
            base.package = package;
            _textManager = package.TextManager;
            _editorAdaptersFactory = package.EditorAdaptersFactory;
            _statusBar = new StatusBarIntegration(package.IStatusBar);
            _envDTE = package.EnvDTE;
            _workspace = package.Workspace;
        }

        public override void BeforeQuery(object sender, EventArgs e)
        {
            base.BeforeQuery(sender, e);

            OleMenuCommand menuCommand = sender as OleMenuCommand;
            menuCommand.Text = "Rename Query";
            menuCommand.Visible = false;
            menuCommand.Enabled = false;

            if (_documentPropertiesProvider.GetContentType() == "CSharp")
            {
                var validator = _documentProcessor.GetValidator();

                menuCommand.Visible = true;
                menuCommand.Enabled = validator.CanRenameQueryInLin(_documentPropertiesProvider.GetSelectionLineNumber());
            }
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            _documentProcessor.TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue);
            _finalActionFactory
                       .GetFinalRenameQueryActionsExecutor(_statusBar, _commandWindow, package.EnvDTE, package.Workspace)
                       .Execute(queryValue, expressionResult);
        }
    }
}
