//------------------------------------------------------------------------------
// <copyright file="GotoAsyncPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using NLog;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using IBatisSuperHelper.Constants;
using IBatisSuperHelper.EventHandlers;
using IBatisSuperHelper.Events;
using IBatisSuperHelper.Helpers;
using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Loggers;
using IBatisSuperHelper.Logging;
using IBatisSuperHelper.Parsers;
using IBatisSuperHelper.Storage;
using IBatisSuperHelper.VSIntegration.Navigation;
using IBatisSuperHelper.Windows.RenameWindow;
using static IBatisSuperHelper.Events.VSSolutionEventsHandler;

namespace IBatisSuperHelper
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
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]   
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GotoAsyncPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(ResultWindow))]
    public sealed class GotoAsyncPackage : AsyncPackage
    {
        /// <summary>
        /// GotoAsyncPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "0d0c7a5a-951b-4d78-ab1d-870fa377376f";

        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// </summary>
        public GotoAsyncPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members
        //Event related fields
        private SolutionEventsHandler _solutionEventsHandler;
        private uint _solutionEventsCookie;
        private Events2 _envDteEvents;
        private ProjectItemsEvents _envDteProjectItemsEvents;
        private EnvDTE.BuildEvents _buildEvents;
        //Public fields
        public IVsSolution Solution;
        public static DTE2 EnvDTE;
        public IVsTextManager TextManager;
        public IVsEditorAdaptersFactoryService EditorAdaptersFactory;
        public IVsStatusbar IStatusBar;
        public ToolWindowPane ResultWindow;
        public Window SolutionExplorer;
        public VisualStudioWorkspace Workspace;
        public static GotoAsyncPackage Instance; 

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            NLogConfigurationService.ConfigureNLog();
            NLogConfigurationService.ConfigureMiniProfilerWithDefaultLogger();

            Logger logger = LogManager.GetLogger("error");
            logger.Info("Extension initalizing");

            EnvDTE = await GetServiceAsync(typeof(DTE)) as DTE2;
            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;

            //Initialize public components, initialize instances that are dependent on any component
            TextManager = await GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            EditorAdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            ResultWindow = FindToolWindow(typeof(ResultWindow), 0, true);

            DocumentNavigationInstance.InjectDTE(GotoAsyncPackage.EnvDTE);
            //Prepare package events
            Workspace = componentModel.GetService<VisualStudioWorkspace>();
            Workspace.WorkspaceChanged += WorkspaceEvents.WorkspaceChanged;

            Observable.FromEventPattern<WorkspaceChangeEventArgs>(Workspace, "WorkspaceChanged")
               //.Select(e => e.EventArgs.Changes)
               .DistinctUntilChanged()
               .Throttle(TimeSpan.FromMilliseconds(500))
               .Subscribe(e =>
               {

               });

          

            _envDteEvents = EnvDTE.Events as Events2;
            if (_envDteEvents != null)
            {
                ProjectItemEventsEx projectItemEvents = new ProjectItemEventsEx();
                _envDteProjectItemsEvents = _envDteEvents.ProjectItemsEvents;
                _envDteProjectItemsEvents.ItemAdded += projectItemEvents.ItemAdded;
                _envDteProjectItemsEvents.ItemRemoved += projectItemEvents.ItemRemoved;
                _envDteProjectItemsEvents.ItemRenamed += projectItemEvents.ItemRenamed;

                EventHandlers.BuildEvents buildEvents = new EventHandlers.BuildEvents();
                _buildEvents = _envDteEvents.BuildEvents;
                _buildEvents.OnBuildBegin += buildEvents.OnBuildBegin;
                
            }

            OutputWindowLogger.Init(await GetServiceAsync(typeof(SVsOutputWindow)) as SVsOutputWindow);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IStatusBar = await GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            Solution = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            _solutionEventsHandler = new SolutionEventsHandler(
                new Action<EventConstats.VS.SolutionLoad>(HandleSolutionEvent),
                Solution
            );
            Solution.AdviseSolutionEvents(_solutionEventsHandler, out _solutionEventsCookie);

            Goto.Initialize(this);
            RenameModalWindowCommand.Initialize(this);
            RenameCommand.Initialize(this);
            Instance = this;
        }

        internal void HandleSolutionEvent(EventConstats.VS.SolutionLoad eventNumber)
        {
            switch (eventNumber)
            {
                case EventConstats.VS.SolutionLoad.SolutionLoadComplete:
                    ThreadHelper.ThrowIfNotOnUIThread();
                    var projectItemHelper = new ProjectItemHelper();
                    var projectItems = projectItemHelper.GetProjectItemsFromSolutionProjects(EnvDTE.Solution.Projects);
                    XmlIndexer xmlIndexer = new XmlIndexer();
                    var xmlFiles = DocumentHelper.GetXmlFiles(projectItems);
                    var xmlIndexerResult = xmlIndexer.BuildIndexerAsync(xmlFiles);
                    PackageStorage.XmlQueries.AddMultipleWithoutKey(xmlIndexerResult);
                    break;
                case EventConstats.VS.SolutionLoad.SolutionOnClose:
                    PackageStorage.CodeQueries.Clear();
                    PackageStorage.XmlQueries.Clear();
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
