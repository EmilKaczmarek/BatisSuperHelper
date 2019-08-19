using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using BatisSuperHelper.VSIntegration;
using NLog;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Loggers;

namespace BatisSuperHelper.Actions
{
    public class QueryRenameActions : BaseActions
    {
        public QueryRenameActions(GotoAsyncPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
        {
            base.Package = package;
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
                menuCommand.Enabled = validator.CanRenameQueryInLine(_documentPropertiesProvider.GetSelectionLineNumber());
            }
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                _documentProcessor.TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue);
                _finalActionFactory
                           .GetFinalRenameQueryActionsExecutor(StatusBar, _commandWindow, GotoAsyncPackage.EnvDTE, Package.Workspace)
                           .Execute(queryValue, expressionResult);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("error").Error(ex, "QueryRenameActions.MenuItemCallback");
                OutputWindowLogger.WriteLn($"Exception occured during QueryRenameActionsMenuCallback: {ex.Message}");
            }
            
        }
    }
}
