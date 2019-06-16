using EnvDTE;
using EnvDTE80;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.HelpersAndExtensions.VisualStudio;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Windows.RenameWindow.ViewModel;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Actions.FinalActions.SubActions.Logic.Rename
{
    public class RenameXmlLogicHandler
    {
        public bool ExecuteRename(Statement query, RenameViewModel renameViewModel, DTE2 envDte)
        {
            try
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                var projectItem = envDte.Solution.FindProjectItem(query.QueryFileName);
                var isProjectItemOpened = projectItem.IsOpen;
                if (!isProjectItemOpened)
                {
                    projectItem.Open();
                }

                var textSelection = projectItem.Document.Selection as TextSelection;
                textSelection.GotoLine(query.QueryLineNumber, true);

                var line = textSelection.GetText();
                line = line.Replace(MapNamespaceHelper.GetQueryWithoutNamespace(query.QueryId), MapNamespaceHelper.GetQueryWithoutNamespace(renameViewModel.QueryText));

                textSelection.Insert(line, (int)vsInsertFlags.vsInsertFlagsContainNewText);
                projectItem.Document.Save();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
