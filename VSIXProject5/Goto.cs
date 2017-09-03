//------------------------------------------------------------------------------
// <copyright file="Goto.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using VSIXProject5.Helpers;
using Microsoft.VisualStudio.LanguageServices;
using System.IO;

namespace VSIXProject5
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Goto
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ae46308e-e071-4943-91dc-3c7733f41554");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Goto(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Goto Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new Goto(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void MenuItemCallback(object sender, EventArgs e)
        {
            IVsTextManager textManager = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView textView = null;
            textManager.GetActiveView(1, null, out textView);
            int selectionLineNum;
            int selectionCol;
            textView.GetCaretPos(out selectionLineNum, out selectionCol);
            IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
            var componentService = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
            SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
            Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            SemanticModel semModel = await doc.GetSemanticModelAsync();
            //doc.TryGetSemanticModel(out semModel);
            NodeHelpers helper = new NodeHelpers(semModel);
            SyntaxTree synTree = null;
            doc.TryGetSyntaxTree(out synTree);
            var span=synTree.GetText().Lines[selectionLineNum].Span;
            var root = (CompilationUnitSyntax)synTree.GetRoot();
            var nodesAtLine = from method in root.DescendantNodesAndSelf(span)
                              select method;
            string title = "No iBatis method at cursor location";
            string queryName = "";
            var returnNode = helper.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
            //Check if current document line is having 'return' keyword.
            //In this case we need to Descendant Node to find ArgumentList
            if (returnNode != null)
            {
                var ReturnNodeDescendanted = returnNode.DescendantNodesAndSelf();
                queryName = helper.GetQueryStringFromSyntaxNodes(ReturnNodeDescendanted);
            }
            //In case we don't have cursor around 'return', SyntaxNodes taken from line
            //should have needed ArgumentLineSyntax 
            else
            {
                queryName = helper.GetQueryStringFromSyntaxNodes(nodesAtLine);
            }
            //We have Query Name, time to find this specific query in Solution files.
            //Theoretic logic is to look only at *.xml files.
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            List<String> projectsPaths = new List<string>();
            foreach(var project in workspace.CurrentSolution.Projects)
            {
                projectsPaths.Add(project.FilePath);
            }
            string path = Path.GetDirectoryName(projectsPaths.First());
            var dirs = Directory.EnumerateFiles(path);


            title = "iBatis method call found in return statment. Of name: " + queryName;
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
