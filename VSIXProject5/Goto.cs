//------------------------------------------------------------------------------
// <copyright file="Goto.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.Indexers;
using VSIXProject5.Models;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;

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

        private DTE2 dte;
        private Events _dteEvents;

        private Events2 _dteEvents2;
        private ProjectItemsEvents _projectItemEvents2;
        private TextDocumentKeyPressEvents _textDocumentKeyPressEvents;
        private StatusBarIntegration _statusBar;

        private void SetupEvents()
        {
            this.dte = this.ServiceProvider.GetService(typeof(DTE)) as DTE2;
            //This is fucking bullshit..
            _dteEvents = dte.Events;
            _dteEvents2 = dte.Events as Events2;
            if (_dteEvents2 != null)
            {
                _projectItemEvents2 = _dteEvents2.ProjectItemsEvents;
                _projectItemEvents2.ItemAdded += _projectItemEvents2_ItemAdded;
                _projectItemEvents2.ItemRemoved += _projectItemEvents2_ItemRemoved;
                _projectItemEvents2.ItemRenamed += _projectItemEvents2_ItemRenamed;

                _textDocumentKeyPressEvents = _dteEvents2.TextDocumentKeyPressEvents;
                _textDocumentKeyPressEvents.AfterKeyPress += TextDocumentKeyPressEvents_AfterKeyPress;
            }

            _timer = new System.Windows.Forms.Timer
            {
                Interval = 100,//ms
            };
            _timer.Tick += _timer_Tick;

            _statusBar = new StatusBarIntegration(this.ServiceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar);
        }

        private void _findEvents_FindDone(vsFindResult Result, bool Cancelled)
        {
            var cos = true;
        }

        private void TextDocumentKeyPressEvents_AfterKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion)
        {
            _timer.Stop();
            _timer.Start();
            var StartPointParent = Selection.Parent.Parent;
            EditedDocument = StartPointParent != null ? (EnvDTE.TextDocument)Selection.Parent.Parent.Object("TextDocument") : (EnvDTE.TextDocument)dte.ActiveDocument.Object("TextDocument");
        }

        private void _projectItemEvents2_ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            Indexer.RenameStatmentsFile(OldName, ProjectItem.Name);        
        }

        private void _projectItemEvents2_ItemRemoved(ProjectItem ProjectItem)
        {
            string fileName = ProjectItem.Name;
            Indexer.RemoveStatmentsForFile(fileName, Path.GetExtension(fileName)==".cs");
        }

        private void _projectItemEvents2_ItemAdded(ProjectItem ProjectItem)
        {
            EnvDTE.Document newDocument = ProjectItem.GetDocumentOrDefault();
            string projectItemExtension = Path.GetExtension(ProjectItem.Name);
            string projectItemPath = ProjectItem.FileNames[0];
            if (projectItemExtension == ".cs")
            {
                CSharpIndexer csIndexer = new CSharpIndexer();
                var buildTask = csIndexer.BuildFromFileAsync(Tuple.Create(projectItemPath, ProjectItem.ContainingProject.Name), Path.GetFileNameWithoutExtension(dte.Solution.FileName));
                Indexer.Build(buildTask.Result);
            }
            else if(projectItemExtension == ".xml")
            {
                XmlIndexer indexerInstance = new XmlIndexer();
                var xmlIndexer = indexerInstance.BuildFromFile(newDocument.FullName);
                Indexer.Build(xmlIndexer);
            }
        }

        private EnvDTE.TextDocument EditedDocument;
        private System.Windows.Forms.Timer _timer;

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (EditedDocument != null)//Edited Document should never be null.
            {            
                string docLanguage = EditedDocument.Language;
                //if (EditedDocument.Selection.IsEmpty) return;
                if (docLanguage == "XML")
                {
                    string documentText = EditedDocument.GetText();

                    var xmlDoc = XDocument.Parse(documentText).DescendantNodes();
                    bool isIBatisQueryXmlFile = XDocHelper.GetXDocumentNamespace(xmlDoc) == IBatisConstants.SqlMapNamespace;
                    if (isIBatisQueryXmlFile)
                    {
                        var newStatments = new XmlIndexer().BuildFromXDocString(documentText, EditedDocument.Parent.FullName, Path.GetDirectoryName(dte.Solution.FullName));

                       Indexer.UpdateXmlStatmentForFile(newStatments);
                    }
                }
                else if (docLanguage == "CSharp")
                {
                    IVsTextManager textManager = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
                    IVsTextView textView = null;
                    textManager.GetActiveView(1, null, out textView);
                    IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
                    var componentService = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
                    SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
                    Microsoft.CodeAnalysis.Document roslynDocument = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
                   
                    var simpleProjectItem = new SimpleProjectItem {
                        FilePath = EditedDocument.Parent.FullName,
                        ProjectName = EditedDocument.Parent.ProjectItem.ContainingProject.Name,
                        IsCSharpFile = true,
                    };

                    var csIndexer = new CSharpIndexer().BuildFromDocumentAsync(roslynDocument, simpleProjectItem, Path.GetFileNameWithoutExtension(dte.Solution.FullName));
                    Indexer.UpdateCodeStatmentForFile(csIndexer.Result);  
                }
            }
        }

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

            SetupEvents();

            DocumentNavigationInstance.InjectDTE(this.dte);

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                var menuItem2 = new OleMenuCommand(new EventHandler(this.MenuItemCallback),
                    new EventHandler(this.Change), new EventHandler(this.BeforeQuery), menuCommandID, "Go to Query");

                commandService.AddCommand(menuItem2);
            }
            
            XmlIndexer indexerInstance = new XmlIndexer();
            ActivityLog.LogInformation("solutionDir:", dte.Solution.FullName);
        }

        //Action before opening menu
        private async void BeforeQuery(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            myCommand.Visible = false;
            string activeDocumentLanguage = dte.ActiveDocument.Language;
            if (activeDocumentLanguage == "CSharp")
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
                var returnNode = helper.GetFirstNodeOfReturnStatmentSyntaxType(nodesAtLine);
                if(returnNode != null)
                {
                    nodesAtLine = returnNode.DescendantNodesAndSelf();
                }
                myCommand.Visible = helper.IsAnySyntaxNodeContainIBatisNamespace(nodesAtLine);
                myCommand.Text = "Go to Query";
            }
            else if (activeDocumentLanguage == "XML")
            {
                EnvDTE.TextDocument doc = (EnvDTE.TextDocument)dte.ActiveDocument.Object("TextDocument");
                TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
                TextPoint pnt = (TextPoint)sel.ActivePoint;
                int oldLineCharBeginOffset = pnt.LineCharOffset;
                int oldLineCharEndOffset = sel.AnchorPoint.LineCharOffset;
                sel.GotoLine(pnt.Line, true);
                var lineText = sel.Text;
                if (lineText.Trim() != "" && !lineText.StartsWith(@"<!--") && lineText.Trim() != "-->")
                {
                    sel.MoveToLineAndOffset(pnt.Line, oldLineCharBeginOffset);
                    sel.MoveToLineAndOffset(pnt.Line, oldLineCharEndOffset, true);

                    string text = doc.GetText();
                    var xmlDoc = XDocument.Parse(text).DescendantNodes();

                    bool isIBatisQueryXmlFile = ((XElement)xmlDoc.First()).Name.NamespaceName == @"http://ibatis.apache.org/mapping";
                    if (isIBatisQueryXmlFile)
                    {
                        myCommand.Visible = true;
                        myCommand.Text = "Go to Query execution";
                    }
                }
            }
        }

        //Unused, but cant remove it
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
            string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

            bool isXmlFile = dte.ActiveDocument.Language == "XML";
            IVsTextManager textManager = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
            IVsTextView textView = null;
            textManager.GetActiveView(1, null, out textView);
            int selectionLineNum;
            int selectionCol;
            textView.GetCaretPos(out selectionLineNum, out selectionCol);
            if (isXmlFile)
            {
                EnvDTE.TextDocument doc = (EnvDTE.TextDocument)(dte.ActiveDocument.Object("TextDocument"));
                string xmlTextContent = doc.GetText();

                var doc2 = XDocument.Parse(xmlTextContent, LoadOptions.SetLineInfo);
                var node = doc2.Descendants();
                var emm = node.Select(x => ((IXmlLineInfo)x).LineNumber).ToList();
                int lineNumber = selectionLineNum + 1;//Missmatch between visual studio lines numeration and text lines numeration
                int? elementLocation = null;
                //This is fucked up let me explain:
                //I can't use .FirstOrDefault, it will give me 0 when no match. But 0 is legit index/location.
                //Logic here is: If user clicks INSIDE tag, it will go to tag declaration that has statment name.
                try
                {
                    elementLocation = emm.First(x => x == lineNumber);
                }
                catch (InvalidOperationException)
                {
                    elementLocation = -1;
                }
                if (elementLocation == -1)
                {
                    emm.Add(lineNumber);
                    emm.Sort();
                    int indexOfLineNumber = emm.IndexOf(lineNumber);
                    elementLocation = emm[indexOfLineNumber == 0 ? 0 : indexOfLineNumber - 1];
                }
                var nodee = node.FirstOrDefault(x => ((IXmlLineInfo)x).LineNumber == elementLocation);
                var queryName = nodee.FirstAttribute.Value;

                var statmentsKeys = Indexer.GetCodeKeysByQueryId(queryName);
                ToolWindowPane window = package.FindToolWindow(typeof(ResultWindow), 0, true);
                if (null == window || null == window.Frame)
                    throw new NotSupportedException("MyToolWindow not found");

                var windowContent = (ResultWindowControl)window.Content;
                var statmentsToShow = statmentsKeys.Select(x => Indexer.GetCodeStatments(x)).ToList().SelectMany(x => x).ToList();
                windowContent.ShowResults(statmentsToShow);
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;

                if (statmentsKeys.Count > 1)
                {
                    _statusBar.ShowText($"Multiple occurence of same statment({queryName}) in solution.");
                    ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    return;
                }
                var statmentKey = statmentsKeys.FirstOrDefault();
                if (statmentKey != null)
                {
                    var statments = Indexer.GetCodeStatmentOrNull(statmentKey);
                    if (statments.Count == 1)
                    {
                        var statment = statments.FirstOrDefault();
                        dte.ItemOperations.OpenFile(statment.QueryFilePath);
                        TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
                        TextPoint pnt = (TextPoint)sel.ActivePoint;
                        sel.GotoLine(statment.QueryLineNumber, true);
                    }
                    else if (statments.Count > 1)
                    {
                        _statusBar.ShowText("Multiple occurence of same statment for given project. Unimplemented");
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                        return;
                    }
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in Code.");
                }
            }
            else
            {
                IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
                var componentService = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
                SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
                Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
                SemanticModel semModel = await doc.GetSemanticModelAsync();
                ActivityLog.TryLogInformation("GoToQuery", "Step 1 executes");
                NodeHelpers helper = new NodeHelpers(semModel);
                SyntaxTree synTree = null;
                doc.TryGetSyntaxTree(out synTree);
                var span = synTree.GetText().Lines[selectionLineNum].Span;
                var root = (CompilationUnitSyntax)synTree.GetRoot();
                var nodesAtLine = from method in root.DescendantNodesAndSelf(span)
                                  select method;
                ActivityLog.TryLogInformation("GoToQuery", "Step 2 executes");
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
                var keys = Indexer.GetXmlKeysByQueryId(queryName);
                if (keys.Any())
                {
                    ToolWindowPane window = package.FindToolWindow(typeof(ResultWindow), 0, false);
                    if (null == window || null == window.Frame)
                        throw new NotSupportedException("MyToolWindow not found");

                    var windowContent = (ResultWindowControl)window.Content;
                    var statmentsToShow = keys.Select(x => Indexer.GetXmlStatment(x)).ToList();
                    windowContent.ShowResults(statmentsToShow);
                    IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;

                    if (keys.Count == 1) { 
                        var statment = Indexer.GetXmlStatmentOrNull(keys.First());
                        dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
                        var obj = dte.ActiveWindow.Object as UIHierarchy;
                        dte.ItemOperations.OpenFile(statment.QueryFilePath); 
                        TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
                        TextPoint pnt = (TextPoint)sel.ActivePoint;
                        sel.GotoLine(statment.QueryLineNumber, true);
                    }
                    else
                    {
                        //var statment = Indexer.GetXmlStatmentOrNull(keys.First());
                        //dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
                        //var obj = dte.ActiveWindow.Object as UIHierarchy;
                        //dte.ItemOperations.OpenFile(statment.QueryFilePath);
                        //TextSelection sel = (TextSelection)dte.ActiveDocument.Selection;
                        //TextPoint pnt = (TextPoint)sel.ActivePoint;
                        //sel.GotoLine(statment.QueryLineNumber, true);
                        _statusBar.ShowText($"Multiple statments of name:{queryName}, jumped to first occurence. Window is WIP");
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    }
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in SqlMaps.");
                }
            }
        }

        private void OnFindDone(vsFindResult Result, bool Cancelled)
        {
            var resultCopy = Result;
        }

    }
}