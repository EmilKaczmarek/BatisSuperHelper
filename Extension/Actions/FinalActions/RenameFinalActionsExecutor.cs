﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using EnvDTE;
using EnvDTE80;
using BatisSuperHelper.Actions.FinalActions.SubActions.Data;
using BatisSuperHelper.Actions.FinalActions.SubActions.Logic.Rename;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.HelpersAndExtensions.VisualStudio;
using BatisSuperHelper.Indexers.Models;
using BatisSuperHelper.Windows.RenameWindow;
using BatisSuperHelper.Windows.RenameWindow.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.LanguageServices;
using BatisSuperHelper.Parsers.Models.SqlMap;

namespace BatisSuperHelper.Actions.FinalActions
{
    public class RenameFinalActionsExecutor : BaseFinalEventActionsExecutor<RenameFinalActionsExecutor>
    {
        public static RenameFinalActionsExecutor Create() => new RenameFinalActionsExecutor();

        private CodeQueryDataService _codeQueryDataService => QueryDataServices[typeof(CSharpQuery)];
        private XmlQueryDataService _xmlQueryDataService => QueryDataServices[typeof(Statement)];
        private DTE2 _envDte;
        private VisualStudioWorkspace _workspace;
        private RenameCodeLogicHandler _codeLogicHandler;
        private RenameXmlLogicHandler _xmlLogicHandler;

        public RenameFinalActionsExecutor WithCodeLogicHandler(RenameCodeLogicHandler logicHandler)
        {
            _codeLogicHandler = logicHandler;
            return this;
        }

        public RenameFinalActionsExecutor WithXmlLogicHandler(RenameXmlLogicHandler logicHandler)
        {
            _xmlLogicHandler = logicHandler;
            return this;
        }

        public RenameFinalActionsExecutor WithEnvDte(DTE2 dte)
        {
            _envDte = dte;
            return this;
        }

        public RenameFinalActionsExecutor WithWorkspace(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
            return this;
        }

        public override void Execute(string queryResult, ExpressionResult expressionResult)
        {
            var codeKeyValuePairs = _codeQueryDataService.GetKeyStatmentPairs(queryResult, UseNamespace);
            var xmlKeys = _xmlQueryDataService.GetStatmentKeys(queryResult, UseNamespace);

            var namespaceQueryPair = MapNamespaceHelper.DetermineMapNamespaceQueryPairFromCodeInput(queryResult);

            RenameModalWindowControl window = new RenameModalWindowControl(
                new RenameViewModel
                {
                    QueryText = namespaceQueryPair.Item2,
                    Namespace = string.IsNullOrEmpty(namespaceQueryPair.Item1) ? null : namespaceQueryPair.Item1,
                });

            window.ShowModal();

            var returnViewModel = window.DataContext as RenameViewModel;

            if (returnViewModel.WasInputCanceled || returnViewModel.QueryText == queryResult)
            {
                return;
            }

            foreach (var key in xmlKeys)
            {
                var query = _xmlQueryDataService.GetSingleStatmentFromKey(key);
                if (_xmlLogicHandler.ExecuteRename(query, returnViewModel, _envDte))
                {
                    _xmlQueryDataService.Rename(key, returnViewModel.QueryText);
                }
            }

            foreach (var keyQueryPair in codeKeyValuePairs)
            {
                if (_codeLogicHandler.ExecuteRename(keyQueryPair, returnViewModel, _workspace))
                {
                    _codeQueryDataService.Rename(keyQueryPair.Key, returnViewModel.QueryText);
                }
            }
        }
    }
}
