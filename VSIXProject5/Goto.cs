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
using EnvDTE80;
using EnvDTE;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Search;

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
        public static readonly Guid CommandSet = new Guid("74b835d6-70a4-4629-9d2c-520ce16236b5");

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
                var menuItem2 = new OleMenuCommand(new EventHandler(this.MenuItemCallback), new EventHandler(this.Change), new EventHandler(this.BeforeQuery), menuCommandID, "Go to Query");

                commandService.AddCommand(menuItem2);

            }
        }
        private async void BeforeQuery(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            myCommand.Text = "Everything fine";
            var events = e;
            IVsTextManager textManager = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView textView = null;
            textManager.GetActiveView(1, null, out textView);
            int selectionLineNum;
            int selectionCol;
            textView.GetCaretPos(out selectionLineNum, out selectionCol);
            IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
            var componentService = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
            SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
            Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (doc == null)
            {
                myCommand.Visible = true;
                myCommand.Text = "Something went wrong";
                return;
            }
            SemanticModel semModel = await doc.GetSemanticModelAsync();
            NodeHelpers helper = new NodeHelpers(semModel);
            SyntaxTree synTree = null;
            doc.TryGetSyntaxTree(out synTree);
            var span = synTree.GetText().Lines[selectionLineNum].Span;
            var root = (CompilationUnitSyntax)synTree.GetRoot();
            var nodesAtLine = from method in root.DescendantNodesAndSelf(span)
                              select method;

            myCommand.Visible = helper.IsAnySyntaxNodeContainIBatisNamespace(nodesAtLine);
            myCommand.Visible = true;
        }
        private void Change(object sender, EventArgs e)
        {

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
            DTE2 dte = this.ServiceProvider.GetService(typeof(DTE)) as DTE2;
            bool isXmlFile = dte.ActiveDocument.Language == "XML";
            IVsTextManager textManager = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView textView = null;
            textManager.GetActiveView(1, null, out textView);
            int selectionLineNum;
            int selectionCol;
            textView.GetCaretPos(out selectionLineNum, out selectionCol);
            if (isXmlFile)
            {
                EnvDTE.TextDocument doc3 = (EnvDTE.TextDocument)(dte.ActiveDocument.Object("TextDocument"));
                var p = doc3.StartPoint.CreateEditPoint();
                string s = p.GetText(doc3.EndPoint);
                sqlMap sqlMapObject = null;
                //using (TextReader readerx = new StreamReader(@"G:\programowanie\iBatis Helper\sqlMap2.xml"))
                //{
                //    XmlSerializer serializer = new XmlSerializer(typeof(sqlMap));
                //    sqlMapObject=(sqlMap)serializer.Deserialize(readerx);
                //}
                //TextReader reader = new StreamReader(@"G:\programowanie\iBatis Helper\sqlMap2.xml");
                var xmlTextContent = s;
                var doc2 = XDocument.Parse(xmlTextContent, LoadOptions.SetLineInfo);
                var node = doc2.Descendants();
                var emm = node.Select(x => ((IXmlLineInfo)x).LineNumber).ToList();
                int lineNumber = selectionLineNum;
                emm.Add(lineNumber);
                emm.Sort();
                int elementLocation = emm[emm.IndexOf(lineNumber) - 1];
                var nodee = node.FirstOrDefault(x => ((IXmlLineInfo)x).LineNumber == elementLocation);
                var nn = nodee.FirstAttribute.Value;
                var findObject2 = dte.Find;
                findObject2.FindWhat = nn;
                findObject2.FilesOfType = "*.cs";
                findObject2.Target = vsFindTarget.vsFindTargetSolution;
                //findObject.Action = vsFindAction.vsFindActionFindAll;
                var findResults2 = findObject2.Execute();
                return;
            }
            IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
            var componentService = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
            SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
            Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
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

            //var testdoc = dte.Solution.FindProjectItem("sqlMap.xml");
            //testdoc.DTE.ItemOperations.OpenFile(testdoc.FileNames[0], EnvDTE.Constants.vsDocumentKindHTML);
            //TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
            //sel.GotoLine(1);
            var xmlSolutionFiles = dte.Solution.Projects;
            List<DocumentProperties> files = new List<DocumentProperties>();
            foreach (EnvDTE.Project project in xmlSolutionFiles)
            {
                var solutionName = dte.Solution.Properties.Item("Name").Value;
                var projectName = project.Name;
                foreach (ProjectItem item in project.ProjectItems)
                {
                    var properies = item.Properties;
                    files.Add(new DocumentProperties
                    {
                        FileName = (string)properies.Item("FileName").Value,
                        FilePath = (string)properies.Item("FullPath").Value,
                        RelativePath = $"{solutionName}\\{projectName}\\{(string)properies.Item("FileName").Value}",
                });
                }
            }
            XmlSearcher searcher = new XmlSearcher();
            var results = searcher.SearchInFiles(queryName, files.Where(x=>x.FileName.Contains("xml")).ToList());
            var firstResult = results.FirstOrDefault();
              
            dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
            var obj = dte.ActiveWindow.Object as UIHierarchy;
            obj.GetItem(firstResult.RelativeVsPath).Select(vsUISelectionType.vsUISelectionTypeSelect);
            obj.DoDefaultAction();
            TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
            TextPoint pnt = (TextPoint)sel.ActivePoint;
            sel.GotoLine(firstResult.SearchLocations.FirstOrDefault().LineNumber, true);
            //var obj = dte.ActiveWindow.Object as UIHierarchy;
            //obj.GetItem(@"IBatisSample\IBatisSample\sqlMap.xml").Select(vsUISelectionType.vsUISelectionTypeSelect);
            //obj.DoDefaultAction();
            //TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
            //TextPoint pnt = (TextPoint)sel.ActivePoint;
            //sel.GotoLine(2, true);
            //dte.ActiveWindow.Object.GetItem("ConsoleApplication1\WindowsFormsApplication1\Program.cs").Select(vsUISelectionType.vsUISelectionTypeSelect);
            //dte.ActiveWindow.Object.DoDefaultAction();
            //var findObject = dte.Find;
            //findObject.FindWhat = queryName;
            //findObject.FilesOfType = "*.xml";
            //findObject.Target = vsFindTarget.vsFindTargetSolution;
            ////findObject.Action = vsFindAction.vsFindActionFindAll;
            //var findResults = findObject.Execute();

            //    title = "iBatis method call found in return statment. Of name: " + queryName;
            //    string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //    // Show a message box to prove we were here
            //    VsShellUtilities.ShowMessageBox(
            //        this.ServiceProvider,
            //        message,
            //        title,
            //        OLEMSGICON.OLEMSGICON_INFO,
            //        OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void OnFindDone(vsFindResult Result, bool Cancelled)
        {
            var resultCopy = Result;
            //throw new NotImplementedException();
        }
    }
}
