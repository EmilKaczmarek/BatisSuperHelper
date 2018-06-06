//------------------------------------------------------------------------------
// <copyright file="GotoPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VSIXProject5.Helpers;
using VSIXProject5.Indexers;

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
    //[ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]   
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GotoPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(BatisSHelperTestConsole))]
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
        private IVsSolution _solution;
        private uint _solutionEventsCookie;
        public DTE2 _dte;
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Goto.Initialize(this);
            base.Initialize();

            BatisSHelperTestConsoleCommand.Initialize(this);
            ResultWindowCommand.Initialize(this);

            var componentModel2 = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = componentModel2.GetService<VisualStudioWorkspace>();
            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;

            _solution = base.GetService(typeof(SVsSolution)) as IVsSolution;
            _solutionEventsHandler = new SolutionEventsHandler(this);
            _dte = base.GetService(typeof(DTE)) as DTE2;
            _solution.AdviseSolutionEvents(_solutionEventsHandler, out _solutionEventsCookie);
            

        }

        private void Workspace_WorkspaceChanged(object sender, Microsoft.CodeAnalysis.WorkspaceChangeEventArgs e)
        {
            var componentModel2 = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = componentModel2.GetService<VisualStudioWorkspace>();
            var kind = e.Kind;
            if (workspace.CurrentSolution != null)
            {

            }
        }
 
        internal async void HandleSolutionEvent(int eventNumber)
        {
            if (eventNumber == 2)
            {
                Indexer.ClearAll();
            }
        }

        internal async void HandleSolutionEventAsync(int eventNumber)
        {
            Debug.WriteLine(eventNumber);
            if (eventNumber == 1)
            {
                var projectItemHelper = new ProjectItemHelper();
                var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects(_dte.Solution.Projects);

                var simpleSolutionItems = DocumentHelper.GetDocumentsFromProjectItemList(projectItems);

                CSharpIndexer csIndexer = new CSharpIndexer();
                XmlIndexer xmlIndexer = new XmlIndexer();

                var xmlIndexerResult = xmlIndexer.BuildIndexer(simpleSolutionItems.Where(e=>!e.IsCSharpFile).ToList());
                Indexer.Build(xmlIndexerResult);

                var codeIndexerResult = await csIndexer.BuildIndexerAsync(simpleSolutionItems.Where(e => e.IsCSharpFile).ToList());     
                Indexer.Build(codeIndexerResult);

            }
        }
        private SolutionEventsHandler _solutionEventsHandler;

        internal class SolutionEventsHandler : IVsSolutionLoadEvents, IVsSolutionEvents
        {
            private readonly GotoPackage _package;
            internal SolutionEventsHandler(GotoPackage package)
            {
                _package = package;
            }
            public int OnBeforeOpenSolution(string pszSolutionFilename)
            {
                return 0;
            }

            public int OnBeforeBackgroundSolutionLoadBegins()
            {
                return 0;
            }

            public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
            {
                pfShouldDelayLoadToNextIdle = false;
                return 0;
            }

            public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
            { 
                return 0;
            }

            public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
            {
                return 0;
            }

            public int OnAfterBackgroundSolutionLoadComplete()
            {
                var componentModel2 = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var workspace = componentModel2.GetService<VisualStudioWorkspace>();
                if (workspace.CurrentSolution.FilePath != null)
                {
                    _package.HandleSolutionEventAsync(1);
                }
                else
                {
                    var _dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                    var solution = _dte.Solution;
                    if (solution.Projects.Count > 0)
                    {
                        //BUG with workspace.
                        //Sometimes when project build version is != 15.0
                        //Even after all projects are loaded, the roslyn workspace doesn't have any solution.
                        //This is leading to:
                        //1)Unable to retrieve Document.
                        //2)Unable to get Semantic Model in CSharp indexer.
                        //Workaround is to wait till workspace change, and than call indexer.
                        //Ugly as my mother, posible 2nd workaround is to work with build.
                        EventHandler<WorkspaceChangeEventArgs> workspaceEventHandler = null;
                        workspaceEventHandler = (object sender, WorkspaceChangeEventArgs e) =>
                        {
                            var changedWorkspace = componentModel2.GetService<VisualStudioWorkspace>();
                            if (changedWorkspace.CurrentSolution.FilePath != null)
                            {
                                _package.HandleSolutionEventAsync(1);
                                workspace.WorkspaceChanged -= workspaceEventHandler;
                            }
                        };
                    }
                }
                return 1;
            }

            public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
            {
                var componentModel2 = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var workspace = componentModel2.GetService<VisualStudioWorkspace>();
                if (workspace.CurrentSolution.FilePath != null)
                {

                }
                var _dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                var solution = _dte.Solution;
                return 0;
            }

            public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
            {
                return 0;
            }

            public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
            {
                return 0;
            }

            public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
            {
                return 0;
            }

            public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
            {
                return 0;
            }

            public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
            {
                return 0;
            }

            public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
            {
                return 0;
            }

            public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
            {
                _package.HandleSolutionEvent(2);
                return 1;
            }

            public int OnBeforeCloseSolution(object pUnkReserved)
            {
                return 0;
            }

            public int OnAfterCloseSolution(object pUnkReserved)
            {
                return 0;
            }
        }
        #endregion
    }
}
