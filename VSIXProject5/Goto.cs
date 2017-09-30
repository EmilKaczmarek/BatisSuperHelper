//------------------------------------------------------------------------------
// <copyright file="Goto.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using iBatisSuperHelper.Services;
using iBatisSuperHelper.Services.Helpers;
using iBatisSuperHelper.Services.Search;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Document = Microsoft.CodeAnalysis.Document;
using Project = EnvDTE.Project;
using TextDocument = EnvDTE.TextDocument;

namespace iBatisSuperHelper
{
    /// <summary>
    /// Command handler
    /// </summary>
    /// 
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
        private readonly Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Goto(Package package)
        {
            this._package = package ?? throw new ArgumentNullException(nameof(package));

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(CommandSet, CommandId);
                var menuItem2 = new OleMenuCommand(MenuItemCallback, Change, BeforeQuery, menuCommandId, "Go to Query");

                commandService.AddCommand(menuItem2);

            }
        }
        //Action before 
        private async void BeforeQuery(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            var textView = _serviceProviderHelper.GetActiveTextView();

            int selectionLineNum;
            int selectionCol;

            textView.GetCaretPos(out selectionLineNum, out selectionCol);
            
            SnapshotPoint caretPosition = _serviceProviderHelper.GetActiveWpfTextView().Caret.Position.BufferPosition;
            Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

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
                return _package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="serviceProviderHelper">Dupa</param>
        public static void Initialize(Package package, IServiceProviderHelper serviceProviderHelper)
        {
            _serviceProviderHelper = serviceProviderHelper;
            Instance = new Goto(package);
        }

        private static IServiceProviderHelper _serviceProviderHelper;
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void MenuItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            bool isXmlFile = dte.ActiveDocument.Language == "XML";
            var textView = _serviceProviderHelper.GetActiveTextView();
            int selectionLineNum;
            int selectionCol;
            textView.GetCaretPos(out selectionLineNum, out selectionCol);
            if (isXmlFile)
            {
                TextDocument doc3 = (TextDocument)(dte.ActiveDocument.Object("TextDocument"));
                var p = doc3.StartPoint.CreateEditPoint();
                string s = p.GetText(doc3.EndPoint);
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
                var nodeLineInfo = nodee as IXmlLineInfo;
                var linePos = nodeLineInfo.LinePosition;
                //TODO: BUG: what if user clicked in empty line between 2 nodes?
                //Possible to find starting and ending line number for each node

                //var findObject2 = dte.Find;
                //findObject2.FindWhat = nn;
                //findObject2.FilesOfType = "*.cs";
                //findObject2.Target = vsFindTarget.vsFindTargetSolution;
                ////findObject.Action = vsFindAction.vsFindActionFindAll;
                //var findResults2 = findObject2.Execute();
                //Roslyn Method
                List<string> projectFiles = new List<string>();
                var componentModel2 = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var workspace = (Workspace)componentModel2.GetService<VisualStudioWorkspace>();
                foreach (Project project in dte.Solution.Projects)
                {
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        var properies = item.Properties;
                        projectFiles.Add((string)properies.Item("FullPath").Value);
                        if (item.Document != null)
                        {
                            var documentid = workspace.CurrentSolution.GetDocumentIdsWithFilePath(item.Document.FullName).FirstOrDefault();
                            var docc = workspace.CurrentSolution.GetDocument(documentid);
                            SemanticModel semModel2 = await docc.GetSemanticModelAsync();
                            //doc.TryGetSemanticModel(out semModel);
                            SyntaxTree synTree2 = null;
                            docc.TryGetSyntaxTree(out synTree2);
                            var root2 = (CompilationUnitSyntax)synTree2.GetRoot();
                            var nodes = root2.DescendantNodesAndSelf();
                            var iBatisNodes = nodes.
                                Select(x => semModel2.GetTypeInfo(x).Type)
                                .Where(x => x != null && x.ContainingNamespace != null)
                                //.Any(x => x.ContainingNamespace.ToDisplayString().Contains("Batis"));
                                .Where(x => x.ContainingSymbol.ToDisplayString().Contains("Batis"))
                                .Select(x => x).ToList();
                            //var iBatisNodes2 = nodes.
                            //    Where(x =>
                            //    {
                            //        var typeInfo = semModel2.GetTypeInfo(x.Parent);
                            //        if (typeInfo.Type != null
                            //            && typeInfo.Type.ContainingNamespace != null
                            //            && typeInfo.Type.ContainingSymbol.ToDisplayString().Contains("Batis")
                            //            )
                            //        {
                            //            return true;
                            //        }
                            //        return false;
                            //    })
                            //    .ToList();
                            //foreach(var n in nodes)
                            //{
                            //    var type = n.GetType();                               
                            //    if(type == typeof(ArgumentListSyntax))
                            //    {
                            //        var semType = semModel2.GetTypeInfo(n);
                            //        if (semType.Type == null)
                            //        {
                            //            var parentSyntaxNode = n.Parent;
                            //            var semTypeParent= semModel2.GetTypeInfo(parentSyntaxNode);
                            //        }
                            //    }
                            //}
                            var intt = 1;
                            var argumentNodes = nodes
                                .Where(x => x.GetType() == typeof(ArgumentListSyntax))
                                .Select(x => x as ArgumentListSyntax)
                                .Where(x => x.Arguments.Any())
                                .Select(x => x)
                                .ToList();
                            foreach (var n in nodes)
                            {
                                var type = n.GetType();
                                if (type == typeof(ArgumentListSyntax))
                                {
                                    var t = n.Ancestors().ToList();
                                    if (t.Any(x => semModel2.GetTypeInfo(x).Type != null && semModel2.GetTypeInfo(x).Type.ContainingNamespace.ToDisplayString().Contains("Batis")))
                                    {
                                        var done = true;
                                    }
                                }
                            }
                        }
                    }
                }
                //foreach(var file in projectFiles.Where(x => x.Contains(".cs")))
                //{

                //}

                return;
            }
            IComponentModel componentModel = (IComponentModel)ServiceProvider.GetService(typeof(SComponentModel));
            var componentService = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
            SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
            Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            SemanticModel semModel = await doc.GetSemanticModelAsync();
            //doc.TryGetSemanticModel(out semModel);
            NodeHelpers helper = new NodeHelpers(semModel);
            SyntaxTree synTree = null;
            doc.TryGetSyntaxTree(out synTree);
            var span = synTree.GetText().Lines[selectionLineNum].Span;
            var root = (CompilationUnitSyntax)synTree.GetRoot();
            var nodesAtLine = from method in root.DescendantNodesAndSelf(span)
                              select method;
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
            var workspace2 = componentModel.GetService<VisualStudioWorkspace>();
            List<String> projectsPaths = new List<string>();
            foreach (var project in workspace2.CurrentSolution.Projects)
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
            foreach (Project project in xmlSolutionFiles)
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
                        RelativePath = $"{solutionName}\\{projectName}\\{(string)properies.Item("FileName").Value}"
                    });
                }
            }
            XmlSearcher searcher = new XmlSearcher();
            var results = searcher.SearchInFiles(queryName, files.Where(x => x.FileName.Contains("xml")).ToList());
            var firstResult = results.FirstOrDefault();

            dte.Windows.Item(Constants.vsWindowKindSolutionExplorer).Activate();
            var obj = dte.ActiveWindow.Object as UIHierarchy;
            obj.GetItem(firstResult.RelativeVsPath).Select(vsUISelectionType.vsUISelectionTypeSelect);
            obj.DoDefaultAction();
            TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
            TextPoint pnt = sel.ActivePoint;
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
