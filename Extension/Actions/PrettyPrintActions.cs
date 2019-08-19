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
using BatisSuperHelper.Parsers;
using System.IO;
using EnvDTE;
using BatisSuperHelper.HelpersAndExtensions.VisualStudio;
using BatisSuperHelper.Actions.PrettyPrint;

namespace BatisSuperHelper.Actions
{
    public class PrettyPrintActions : BaseActions
    {
        private readonly IPrettyPrintService _prettyPrintService = new PrettyPrintService();

        public PrettyPrintActions(GotoAsyncPackage package) : base(package.TextManager, package.EditorAdaptersFactory, new StatusBarIntegration(package.IStatusBar))
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

            if (_documentPropertiesProvider.GetContentType() == "XML")
            {
                var validator = _documentProcessor.GetValidator();

                menuCommand.Visible = true;
                menuCommand.Enabled = validator.CanJumpToQueryInLine(_documentPropertiesProvider.GetSelectionLineNumber());
            }
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var docContent = base._documentPropertiesProvider.GetDocumentRepresentation();
            var parser = new BatisXmlMapParser().WithStringReader(new StringReader(docContent as string)).Load();
            var statement = parser.GetStatementAtLineOrNull(base._documentPropertiesProvider.GetSelectionLineNumber() + 1);
            var splitedContent = statement.Content.Split(new[]{Environment.NewLine}, StringSplitOptions.None);

            var changedContent = new IndentPerserveService(statement.Content, _prettyPrintService.PrettyPrint(statement.Content))
                .GetFormattedSqlWithOriginalIndents();

            if (changedContent.Equals(statement.Content))
            {
                return;
            }

            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var projectItem = GotoAsyncPackage.EnvDTE.Solution.FindProjectItem(_documentPropertiesProvider.GetPath());
                var isProjectItemOpened = projectItem.IsOpen;
                if (!isProjectItemOpened)
                {
                    projectItem.Open();
                }

                var textSelection = projectItem.Document.Selection as TextSelection;

                TextPoint pnt = textSelection.ActivePoint;
                int oldLineNum = pnt.Line + 1;
                int oldLineCharBeginOffset = pnt.LineCharOffset;
                int oldLineCharEndOffset = textSelection.AnchorPoint.LineCharOffset;

                var newContentSplited = changedContent.Split(new[]{Environment.NewLine}, StringSplitOptions.None);

                textSelection.GotoLine(statement.QueryLineNumber + 1, true);

                textSelection.LineDown(true, splitedContent.Length-1);

                textSelection.Insert(changedContent, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                textSelection.MoveToLineAndOffset(oldLineNum, oldLineCharBeginOffset);
                textSelection.MoveToLineAndOffset(oldLineNum, oldLineCharEndOffset, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
