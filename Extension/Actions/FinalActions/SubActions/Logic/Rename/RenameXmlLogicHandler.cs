﻿using EnvDTE;
using EnvDTE80;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.HelpersAndExtensions.VisualStudio;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Windows.RenameWindow.ViewModel;
using Microsoft.VisualStudio.LanguageServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Actions.FinalActions.SubActions.Logic.Rename
{
    public class RenameXmlLogicHandler
    {
        public bool ExecuteRename(XmlQuery query, RenameViewModel renameViewModel, DTE2 envDte)
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
                line = line.Replace(MapNamespaceHelper.GetQueryWithoutNamespace(query), MapNamespaceHelper.GetQueryWithoutNamespace(renameViewModel.QueryText));

                textSelection.Insert(line, (int)vsInsertFlags.vsInsertFlagsContainNewText);
                projectItem.Document.Save();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}