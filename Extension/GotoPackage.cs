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
using Microsoft;
using IBatisSuperHelper.Indexers.Xml;
using IBatisSuperHelper.HelpersAndExtensions;
using IBatisSuperHelper.EventHandlers.SolutionEventsActions;
using IBatisSuperHelper.Indexers.Workflow;
using IBatisSuperHelper.Indexers.Workflow.Options;
using IBatisSuperHelper.CoreAutomation.ProjectItems;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Config;
using IBatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;

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
        public const string PackageGuidString = "0d0c7a5a-951b-4d78-ab1d-870fa377376f";

        public GotoAsyncPackage()
        {

        }

        public GotoAsyncPackage(IPackageStorage storage)
        {
            _packageStorage = storage;
        }

        #region Package Members
        //Event related fields
        private SolutionEventsHandler _solutionEventsHandler;
        private uint _solutionEventsCookie;
        private Events2 _envDteEvents;
        private ProjectItemsEvents _envDteProjectItemsEvents;
        private EnvDTE.BuildEvents _buildEvents;
        private static IPackageStorage _packageStorage;
        //Public fields
        public IVsSolution Solution;
        public static DTE2 EnvDTE;
        public IVsTextManager TextManager;
        public IVsEditorAdaptersFactoryService EditorAdaptersFactory;
        public IVsStatusbar IStatusBar;
        public ToolWindowPane ResultWindow;
        public Window SolutionExplorer;
        public VisualStudioWorkspace Workspace;
        public static IPackageStorage Storage {
            get
            {
                return _packageStorage ?? new PackageStorage();
            }
            private set
            {
                _packageStorage = value;
            }
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            NLogConfigurationService.ConfigureNLog();
            NLogConfigurationService.ConfigureMiniProfilerWithDefaultLogger();

            Logger logger = LogManager.GetLogger("error");
            logger.Info("Extension initalizing");

            EnvDTE = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(EnvDTE);

            _packageStorage = new PackageStorage();

            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(componentModel);

            //Initialize public components, initialize instances that are dependent on any component
            TextManager = await GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            EditorAdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            ResultWindow = FindToolWindow(typeof(ResultWindow), 0, true);

            DocumentNavigationInstance.InjectDTE(EnvDTE);
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
                ProjectItemEventsActions projectItemEvents = new ProjectItemEventsActions();
                _envDteProjectItemsEvents = _envDteEvents.ProjectItemsEvents;
                _envDteProjectItemsEvents.ItemAdded += projectItemEvents.ItemAdded;
                _envDteProjectItemsEvents.ItemRemoved += projectItemEvents.ItemRemoved;
                _envDteProjectItemsEvents.ItemRenamed += projectItemEvents.ItemRenamed;

                EventHandlers.BuildEventsActions buildEvents = new EventHandlers.BuildEventsActions();
                _buildEvents = _envDteEvents.BuildEvents;
                _buildEvents.OnBuildBegin += buildEvents.OnBuildBegin;
                
            }

            OutputWindowLogger.Init(await GetServiceAsync(typeof(SVsOutputWindow)) as SVsOutputWindow);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IStatusBar = await GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            Solution = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(Solution);

            var indexingWorkflow = new IndexingWorkflow(Storage.IndexingWorkflowOptions, new ProjectItemRetreiver(EnvDTE), Storage);

            _solutionEventsHandler = new SolutionEventsHandler(new VSSolutionEventsActions(indexingWorkflow));
            Solution.AdviseSolutionEvents(_solutionEventsHandler, out _solutionEventsCookie);

            Goto.Initialize(this);
            RenameModalWindowCommand.Initialize(this);
            RenameCommand.Initialize(this);

        }
        #endregion
    }
}
