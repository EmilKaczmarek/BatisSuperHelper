//------------------------------------------------------------------------------
// <copyright file="GotoPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Xml.Linq;
using VSIXProject5.Constants;
using VSIXProject5.EventHandlers;
using VSIXProject5.Events;
using VSIXProject5.Helpers;
using VSIXProject5.Indexers;
using VSIXProject5.VSIntegration.Navigation;
using static VSIXProject5.Events.VSSolutionEventsHandler;

namespace VSIXProject5
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    /// 
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]   
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GotoPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    //[ProvideToolWindow(typeof(BatisSHelperTestConsole))]
    [ProvideToolWindow(typeof(ResultWindow))]
    public sealed class GotoPackage : Package
    {
        /// <summary>
        /// GotoPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "0d0c7a5a-951b-4d78-ab1d-870fa377376f";

        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// </summary>
        public GotoPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members
        //Event related fields
        private IVsSolution _solution;
        private SolutionEventsHandler _solutionEventsHandler;
        private uint _solutionEventsCookie;
        private EnvDTE80.Events2 _envDteEvents;
        private EnvDTE.ProjectItemsEvents _envDteProjectItemsEvents;
        private TextDocumentKeyPressEvents _textDocumentKeyPressEvents;
        private System.Windows.Forms.Timer _timer;
        //Public fields
        public DTE2 EnvDTE;
        public IVsTextManager TextManager;
        public IVsEditorAdaptersFactoryService EditorAdaptersFactory;
        public IVsStatusbar IStatusBar;
        public ToolWindowPane ResultWindow;
        public Window SolutionExplorer;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            //Initialize window
            //BatisSHelperTestConsoleCommand.Initialize(this);
            ResultWindowCommand.Initialize(this);
            //Initialize base components
            EnvDTE = base.GetService(typeof(DTE)) as DTE2;
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            //Initialize public components, initialize instances that are dependent on any component
            TextManager = (IVsTextManager)GetService(typeof(SVsTextManager));
            EditorAdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            IStatusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            ResultWindow = FindToolWindow(typeof(ResultWindow), 0, true);
            DocumentNavigationInstance.InjectDTE(this.EnvDTE);
            //Prepare package events
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            workspace.WorkspaceChanged += WorkspaceEvents.WorkspaceChanged;

            _solution = base.GetService(typeof(SVsSolution)) as IVsSolution;
            _solutionEventsHandler = new SolutionEventsHandler(
                new Action<EventConstats.VS.SolutionLoad>(HandleSolutionEvent)
            );      
            _solution.AdviseSolutionEvents(_solutionEventsHandler, out _solutionEventsCookie);

            _envDteEvents = EnvDTE.Events as Events2;
            if (_envDteEvents != null)
            {
                ProjectItemEventsEx projectItemEvents = new ProjectItemEventsEx();
                _envDteProjectItemsEvents = _envDteEvents.ProjectItemsEvents;
                _envDteProjectItemsEvents.ItemAdded += projectItemEvents.ItemAdded;
                _envDteProjectItemsEvents.ItemRemoved += projectItemEvents.ItemRemoved;
                _envDteProjectItemsEvents.ItemRenamed += projectItemEvents.ItemRenamed;
                //Init text change
                _textDocumentKeyPressEvents = _envDteEvents.TextDocumentKeyPressEvents;
                _textDocumentKeyPressEvents.AfterKeyPress += TextDocumentKeyPressEvents_AfterKeyPress;
            }
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 1000,
            };
            _timer.Tick += _timer_Tick;
            //Initialize commands
            Goto.Initialize(this);
        }

        private EnvDTE.TextDocument _editedDocument;
        private void TextDocumentKeyPressEvents_AfterKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion)
        {
            _timer.Stop();
            _timer.Start();
            var StartPointParent = Selection.Parent.Parent;
            _editedDocument = StartPointParent != null ? (EnvDTE.TextDocument)Selection.Parent.Parent.Object("TextDocument") : (TextDocument)EnvDTE.ActiveDocument.Object("TextDocument");
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (_editedDocument != null)//Edited Document should never be null.
            {
                string docLanguage = _editedDocument.Language;
                if (docLanguage == "XML")
                {
                    string documentText = _editedDocument.GetText();

                    var xDoc = XDocument.Parse(documentText);
                    bool isIBatisQueryXmlFile = XDocHelper.GetXDocumentNamespace(xDoc) == IBatisConstants.SqlMapNamespace;
                    if (isIBatisQueryXmlFile)
                    {
                        var newStatments = new XmlIndexer().BuildFromXDocString(documentText, _editedDocument.Parent.FullName);
                        Indexer.UpdateXmlStatmentForFile(newStatments);
                    }
                }
                else if (docLanguage == "CSharp")
                {
                    //TODO: Check if file has any iBatis usings?
                    TextManager.GetActiveView(1, null, out IVsTextView textView);

                    var componentService = EditorAdaptersFactory.GetWpfTextView(textView);
                    SnapshotPoint caretPosition = componentService.Caret.Position.BufferPosition;
                    Microsoft.CodeAnalysis.Document roslynDocument = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                    var csIndexer = new CSharpIndexer().BuildFromDocumentAsync(roslynDocument).Result;
                    Indexer.UpdateCodeStatmentForFile(csIndexer);
                }
            }
        }

        internal void HandleSolutionEvent(EventConstats.VS.SolutionLoad eventNumber)
        {
            Debug.WriteLine(eventNumber);
            switch (eventNumber)
            {
                case EventConstats.VS.SolutionLoad.SolutionLoadComplete:
                    Debug.WriteLine("Solution loaded event Start");
                    var projectItemHelper = new ProjectItemHelper();
                    var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects(EnvDTE.Solution.Projects);
                    Debug.WriteLine($"Found {projectItems.Count} items.\n {string.Join("\n", projectItems.Select(e => e.Name).ToList())}");
                    XmlIndexer xmlIndexer = new XmlIndexer();
                    var xmlFiles = DocumentHelper.GetXmlFiles(projectItems);
                    Debug.Write(xmlFiles);
                    Debug.WriteLine($"Xml Files: {xmlFiles.Count}.\n {string.Join("\n", xmlFiles.Select(e=>e.FilePath))}");
                    var xmlIndexerResult = xmlIndexer.BuildIndexer(xmlFiles);
                    Indexer.Build(xmlIndexerResult);
                    IndexersProcessStatus.XmlIndexerFinished = true;
                    Debug.WriteLine("Solution loaded event Start");
                    break;
                case EventConstats.VS.SolutionLoad.SolutionOnClose:
                    Indexer.ClearAll();
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
