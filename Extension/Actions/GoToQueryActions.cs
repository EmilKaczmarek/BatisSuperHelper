﻿using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using NLog;
using StackExchange.Profiling;
using System;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Logging.MiniProfiler;
using BatisSuperHelper.VSIntegration;
using BatisSuperHelper.Storage;
using Microsoft.VisualStudio.LanguageServices;
using System.Linq;
using BatisSuperHelper.CoreAutomation.ProjectItems;
using BatisSuperHelper.HelpersAndExtensions;

namespace BatisSuperHelper.Actions
{
    public class GoToQueryActions2 : BaseActions
    {
        GotoAsyncPackage _package;
        public GoToQueryActions2(GotoAsyncPackage package) : 
            base(
                package.TextManager, 
                package.EditorAdaptersFactory, 
                new StatusBarIntegration(package.IStatusBar), 
                new Lazy<ToolWindowPane>(() => package.FindToolWindow(typeof(ResultWindow), 0, true))
           )
        {
            _package = package;
        }

        public override void BeforeQuery(object sender, EventArgs e)
        {
            base.BeforeQuery(sender, e);

            OleMenuCommand menuCommand = sender as OleMenuCommand;
            menuCommand.Text = "Go to Query";
            menuCommand.Visible = false;
            menuCommand.Enabled = false;

            var validator = _documentProcessor.GetValidator();
            bool validationResult = validator.CanJumpToQueryInLine(_documentPropertiesProvider.GetSelectionLineNumber());

            menuCommand.Visible = validationResult;
            menuCommand.Enabled = validationResult;

        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var profiler = MiniProfiler.StartNew($"{nameof(GoToQueryActions2)}.{nameof(MenuItemCallback)}"); 
                profiler.Storage = new NLogStorage(LogManager.GetLogger("profiler"));
                using (profiler.Step("Event start"))   
                {
                    _documentProcessor.TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue);
                    _finalActionFactory
                        .GetFinalGoToQueryActionsExecutor(StatusBar, _commandWindow, MapNamespaceHelper.IsQueryWithNamespace(queryValue))
                        .Execute(queryValue, expressionResult);
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
