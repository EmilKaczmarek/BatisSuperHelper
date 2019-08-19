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
using BatisSuperHelper.Constants;
using BatisSuperHelper.EventHandlers;
using BatisSuperHelper.Events;
using BatisSuperHelper.Helpers;
using BatisSuperHelper.Indexers;
using BatisSuperHelper.Loggers;
using BatisSuperHelper.Logging;
using BatisSuperHelper.Parsers;
using BatisSuperHelper.Storage;
using BatisSuperHelper.VSIntegration.Navigation;
using BatisSuperHelper.Windows.RenameWindow;
using static BatisSuperHelper.Events.VSSolutionEventsHandler;
using Microsoft;
using BatisSuperHelper.Indexers.Xml;
using BatisSuperHelper.HelpersAndExtensions;
using BatisSuperHelper.EventHandlers.SolutionEventsActions;
using BatisSuperHelper.Indexers.Workflow;
using BatisSuperHelper.Indexers.Workflow.Options;
using BatisSuperHelper.CoreAutomation.ProjectItems;
using BatisSuperHelper.Indexers.Workflow.Strategies.Config;
using BatisSuperHelper.Indexers.Workflow.Strategies.Storage.Configs;
using BatisSuperHelper.Indexers.Code;

namespace BatisSuperHelper
{
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GotoAsyncPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(ResultWindow))]
    public sealed class GotoAsyncPackage : AsyncPackage
    {
        public const string PackageGuidString = "0d0c7a5a-951b-4d78-ab1d-870fa377376f";

        public GotoAsyncPackage()
        {

        }

        #region Package Members
        //Event related fields
        private SolutionEventsHandler _solutionEventsHandler;
        private uint _solutionEventsCookie;
        private Events2 _envDteEvents;
        private ProjectItemsEvents _envDteProjectItemsEvents;
        private EnvDTE.BuildEvents _buildEvents;


        public IVsSolution Solution { get; private set; }
        public IVsTextManager TextManager { get; private set; }
        public IVsEditorAdaptersFactoryService EditorAdaptersFactory { get; private set; }
        public IVsStatusbar IStatusBar { get; private set; }
        //public ToolWindowPane ResultWindow { get; private set; }
        public VisualStudioWorkspace Workspace { get; private set; }

        public static SemaphoreSlim DteSemaphore = new SemaphoreSlim(0, 1);
        public static DTE2 EnvDTE { get; private set; }

        private static IPackageStorage _packageStorage;
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

            DteSemaphore.Release();

            Storage = new PackageStorage();

            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(componentModel);

            //Initialize public components, initialize instances that are dependent on any component
            TextManager = await GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            EditorAdaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            DocumentNavigationInstance.InjectDTE(EnvDTE);
            //Prepare package events
            var indexingQueue = new ProjectIndexingQueue();
            var workspaceEvents = new WorkspaceEvents(indexingQueue);

            Workspace = componentModel.GetService<VisualStudioWorkspace>();
            Workspace.WorkspaceChanged += (s,e) => ThreadHelper.JoinableTaskFactory.RunAsync(async () => await workspaceEvents.WorkspaceChangedAsync(s,e));

            var indexingWorkflow = new IndexingWorkflow(Storage.IndexingWorkflowOptions, new ProjectItemRetreiver(EnvDTE), Storage);

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

            var solutionEventsActions = new VSSolutionEventsActions(indexingWorkflow);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await ResultWindowCommand.InitializeAsync(this);
            OutputWindowLogger.Init(await GetServiceAsync(typeof(SVsOutputWindow)) as SVsOutputWindow);
            IStatusBar = await GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            var svsSolution = await GetServiceAsync(typeof(SVsSolution));
            Solution = svsSolution as IVsSolution;
            Assumes.Present(Solution);

            await HandleSolutionAsync(svsSolution, solutionEventsActions, indexingQueue);


            _solutionEventsHandler = new SolutionEventsHandler(solutionEventsActions);
            Solution.AdviseSolutionEvents(_solutionEventsHandler, out _solutionEventsCookie);

            await Goto.InitializeAsync(this);
            await RenameModalWindowCommand.InitializeAsync(this);
            await RenameCommand.InitializeAsync(this);
            await PrettyPrintCommand.InitializeAsync(this);

        }

        private async System.Threading.Tasks.Task HandleSolutionAsync(object solutionService, VSSolutionEventsActions solutionEventsActions, ProjectIndexingQueue indexingQueue)
        {
            var solution = solutionService as IVsSolution;

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object isSolutionOpenedValue);
            var isSolutionOpened = isSolutionOpenedValue is bool isSolutionOpenedBool && isSolutionOpenedBool;
            if (isSolutionOpened)
            {
                solutionEventsActions.OnSolutionLoadComplete();
                await indexingQueue.EnqueueMultipleAsync(Workspace.CurrentSolution.Projects);
            }

        }

        #endregion
    }
}
