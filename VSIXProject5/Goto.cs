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
using System.Xml;
using System.Xml.Linq;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.Indexers;
using VSIXProject5.Models;
using VSIXProject5.VSIntegration;
using VSIXProject5.VSIntegration.Navigation;
using VSIXProject5.HelpersAndExtensions.VisualStudio;

using static VSIXProject5.HelpersAndExtensions.XmlHelper;
using Microsoft.VisualStudio.LanguageServices;
using System.Diagnostics;

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
        private EnvDTE.Events _dteEvents;

        private Events2 _dteEvents2;
        private ProjectItemsEvents _projectItemEvents2;
        private TextDocumentKeyPressEvents _textDocumentKeyPressEvents;
        private StatusBarIntegration _statusBar;

        private IComponentModel _componentModel; 
        private IVsTextManager _textManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Goto(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            this.package = package;

            SetupEvents();

            DocumentNavigationInstance.InjectDTE(this.dte);

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                var menuItem2 = new OleMenuCommand(new EventHandler(this.MenuItemCallback),
                    new EventHandler(this.Change), new EventHandler(this.BeforeQuery), menuCommandID, "Go to Query");

                commandService.AddCommand(menuItem2);
            }

            _componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));
            _textManager = (IVsTextManager)this.ServiceProvider.GetService(typeof(SVsTextManager));
        }

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
            Indexer.RemoveStatmentsForFile(fileName, false);
        }

        private void _projectItemEvents2_ItemAdded(ProjectItem ProjectItem)
        {
            string projectItemExtension = Path.GetExtension(ProjectItem.Name);
            if (projectItemExtension == ".xml")
            {
                XmlIndexer indexerInstance = new XmlIndexer();
                var xmlIndexer = indexerInstance.BuildUsingFilePath(ProjectItem.FileNames[0]);
                Indexer.Build(xmlIndexer);
            }

            //Adding code document is handled by Workspace
            //if (projectItemExtension == ".cs")
            //{
            //    string projectItemPath = ProjectItem.FileNames[0];
            //    CSharpIndexer csIndexer = new CSharpIndexer();
            //    XmlFileInfo simpleProjectItem = new XmlFileInfo
            //    {
            //        FilePath = projectItemPath,
            //        ProjectName = ProjectItem.ContainingProject.Name
            //    };
            //    //var buildTask = csIndexer.BuildFromFileAsync(simpleProjectItem);
            //    //Indexer.Build(buildTask.Result);
            //}

        }

        private EnvDTE.TextDocument EditedDocument;
        private System.Windows.Forms.Timer _timer;

        private async void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (EditedDocument != null)//Edited Document should never be null.
            {            
                string docLanguage = EditedDocument.Language;
                if (docLanguage == "XML")
                {
                    string documentText = EditedDocument.GetText();

                    var xDoc = XDocument.Parse(documentText);
                    bool isIBatisQueryXmlFile = XDocHelper.GetXDocumentNamespace(xDoc) == IBatisConstants.SqlMapNamespace;
                    if (isIBatisQueryXmlFile)
                    {
                        var newStatments = new XmlIndexer().BuildFromXDocString(documentText, EditedDocument.Parent.FullName);
                        Indexer.UpdateXmlStatmentForFile(newStatments);
                    }
                }
                else if (docLanguage == "CSharp")
                {
                    //TODO: Check if file has any iBatis usings?
                    _textManager.GetActiveView(1, null, out IVsTextView textView);

                    var componentService = _componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
                    SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
                    Microsoft.CodeAnalysis.Document roslynDocument = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                    var csIndexer = new CSharpIndexer().BuildFromDocumentAsync(roslynDocument).Result;
                    Indexer.UpdateCodeStatmentForFile(csIndexer);  
                }
            }
        }

        //Action before opening menu
        private void BeforeQuery(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            myCommand.Visible = false;
            string activeDocumentLanguage = dte.ActiveDocument.Language;
            if (activeDocumentLanguage == "CSharp")
            {
                //Get selection line number
                _textManager.GetActiveView(1, null, out IVsTextView textView);
                textView.GetCaretPos(out int selectionLineNum, out int selectionCol);
                //Get carret position
                var wpfTextView = _componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
                SnapshotPoint caretPosition = wpfTextView.Caret.Position.BufferPosition;

                Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                if (doc == null)
                {
                    myCommand.Visible = true;
                    myCommand.Text = "Something went wrong";
                    return;
                }
                //Cool case: there is doc.TryGetSemanticModel but if it's not ready it will be null.
                //GetSemanticModelAsync will create it if needed, but not sure if using async event is good.
                SemanticModel semModel = doc.GetSemanticModelAsync().Result;
                
                NodeHelpers helper = new NodeHelpers(semModel);
                doc.TryGetSyntaxTree(out SyntaxTree synTree);

                var lineSpan = synTree.GetText().Lines[selectionLineNum].Span;
                var treeRoot = (CompilationUnitSyntax)synTree.GetRoot();
                var nodesAtLine = treeRoot.DescendantNodesAndSelf(lineSpan);

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
                var lineText = sel.GetText();

                if (!XmlStringLine.IsIgnored(lineText))
                {
                    string text = doc.GetText();
                    var xDoc = XDocument.Parse(text);

                    bool isIBatisQueryXmlFile = XDocHelper.GetXDocumentNamespace(xDoc) == @"http://ibatis.apache.org/mapping";
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
        
        //Too long and complicated...
        //TODO: Cleanup on result window enchantments.
        //TODO: Cleanup on multiple statments in xml logic.
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        { 
            bool isXmlFile = dte.ActiveDocument.Language == "XML";

            IVsTextView textView = null;
            _textManager.GetActiveView(1, null, out textView);
            int selectionLineNum;
            textView.GetCaretPos(out selectionLineNum, out int selectionCol);
            if (isXmlFile)
            {
                EnvDTE.TextDocument doc = (EnvDTE.TextDocument)(dte.ActiveDocument.Object("TextDocument"));
                string xmlTextContent = doc.GetText();

                var xmlDocument = XDocument.Parse(xmlTextContent, LoadOptions.SetLineInfo);
                var xElements = xmlDocument.Descendants();
                var elementsLineNumbers = xElements.Select(x => ((IXmlLineInfo)x).LineNumber).ToList();
                int lineNumber = selectionLineNum + 1;//Missmatch between visual studio lines numeration and text lines numeration
                int? elementLocation = elementsLineNumbers.Cast<int?>().FirstOrDefault(x => x == lineNumber);

                if (elementLocation == null)
                {
                    elementsLineNumbers.Add(lineNumber);
                    elementsLineNumbers.Sort();
                    int indexOfLineNumber = elementsLineNumbers.IndexOf(lineNumber);
                    elementLocation = elementsLineNumbers[indexOfLineNumber == 0 ? 0 : indexOfLineNumber - 1];
                }

                var node = xElements.FirstOrDefault(x => ((IXmlLineInfo)x).LineNumber == elementLocation);
                var queryName = node.Attributes().FirstOrDefault(x => x.Name.LocalName == IBatisConstants.StatmentIdAttributeName).Value;

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
                        TextPoint pnt = sel.ActivePoint;
                        sel.GotoLine(statment.QueryLineNumber, true);
                    }
                    else if (statments.Count > 1)
                    {
                        _statusBar.ShowText("Multiple occurence of same statment in project.");
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
                var componentService = _componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
                SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
                Microsoft.CodeAnalysis.Document doc = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                SemanticModel semModel = doc.GetSemanticModelAsync().Result;

                NodeHelpers helper = new NodeHelpers(semModel);
                SyntaxTree synTree = null;
                doc.TryGetSyntaxTree(out synTree);
                var span = synTree.GetText().Lines[selectionLineNum].Span;
                var root = (CompilationUnitSyntax)synTree.GetRoot();
                var nodesAtLine = root.DescendantNodesAndSelf(span);
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
                    //This should never happend without implementing logic for multiple xml statments of same name.
                    else
                    {
                        _statusBar.ShowText($"Multiple statments of name:{queryName}");
                        //ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    }
                }
                else
                {
                    _statusBar.ShowText($"No occurence of query named: {queryName} find in SqlMaps.");
                }
            }
        }
    }
}