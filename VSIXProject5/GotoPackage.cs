//------------------------------------------------------------------------------
// <copyright file="GotoPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Microsoft.VisualStudio.TextManager.Interop;
using VSIXProject5.Indexers;
using System.IO;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.LanguageServices;
using System.Text;

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
        private uint _solutionEventsCookie = 0;
        public DTE2 _dte;
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Goto.Initialize(this);
            base.Initialize();
            _solution = base.GetService(typeof(SVsSolution)) as IVsSolution;
            _solutionEventsHandler = new SolutionEventsHandler(this);
            _textManagerEvents = new TextManagerEvents();
            _dte = base.GetService(typeof(DTE)) as DTE2;
            _solution.AdviseSolutionEvents(_solutionEventsHandler, out _solutionEventsCookie);
            //IVsTextManager vsTextManager = base.GetService(typeof(IVsTextManager)) as IVsTextManager;
            //IConnectionPointContainer textManager = (IConnectionPointContainer)GetService(typeof(SVsTextManager));
            //Guid interfaceGuid = typeof(IVsTextManagerEvents).GUID;
            //IConnectionPoint tmConnectionPoint;
            //textManager.FindConnectionPoint(ref interfaceGuid, out tmConnectionPoint);
            //uint cookie;
            //tmConnectionPoint.Advise(_textManagerEvents, out cookie);
            //var textManager = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager)) as IVsTextManager2;
            //IConnectionPointContainer container = textManager as IConnectionPointContainer;
            //IConnectionPoint textManagerEventsConnection = null;
            //Guid eventGuid = typeof(IVsTextManagerEvents2).GUID;
            //container.FindConnectionPoint(ref eventGuid, out textManagerEventsConnection);
            //var textManagerEvents = new TextManagerEvents();
            //uint textManagerCookie;
            //textManagerEventsConnection.Advise(textManagerEvents, out textManagerCookie);
            //var textManager = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            //IVsTextManager2 textLine = (IVsTextManager2)GetService(typeof(SVsTextManager));            
            //IVsTextView actiV;
            //IVsTextView _activeView;
            //textLine.GetActiveView2(0, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out actiV);
            //IVsTextLines _buffer = null;
            //if (textLine.GetActiveView2(0, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out _activeView) == VSConstants.S_OK)
            //{
            //    _activeView.GetBuffer(out _buffer);

            //    string selectedText = string.Empty;
            //    actiV.GetSelectedText(out selectedText);
            //}
            //IConnectionPointContainer container = _buffer as IConnectionPointContainer;
            //IConnectionPoint textManagerEventsConnection = null;
            //Guid eventGuid = typeof(IVsTextLinesEvents).GUID;
            //container.FindConnectionPoint(ref eventGuid, out textManagerEventsConnection);
            //uint textManagerCookie;
            //textManagerEventsConnection.Advise(_textManagerLineEvents, out textManagerCookie);

            BatisSHelperTestConsoleCommand.Initialize(this);
            ResultWindowCommand.Initialize(this);
        }

        public void OnChangeLineText(TextLineChange[] pTextLineChange, int fLast)
        {
            var test1 = 1;
        }

        public void OnChangeLineAttributes(int iFirstLine, int iLastLine)
        {
            var test1 = 1;
        }
        List<ProjectItem> projectItems = new List<ProjectItem>();

        private ProjectItem GetFiles(ProjectItem item)
        {
            if (item.ProjectItems == null)
                return item;

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                projectItems.Add(GetFiles(currentItem));
            }

            return item;
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
                var projects = _dte.Solution.Projects.GetEnumerator();
                if (projects != null)
                {
                    while (projects.MoveNext())
                    {
                        var project = projects.Current as Project;
                        if (project != null && project.ProjectItems != null)
                        {  
                            var items = project.ProjectItems.GetEnumerator();
                            while (items.MoveNext())
                            {
                                var item = (ProjectItem)items.Current;
                                projectItems.Add(GetFiles(item));
                            }
                        }
                    }
                }
                List<Tuple<string, string>> solutionCodeDocuments = new List<Tuple<string, string>>();
                List<Tuple<string, string>> solutionXmlDocument = new List<Tuple<string, string>>();
                foreach (var projectItem in projectItems)
                {
                    Document document = new Func<EnvDTE.Document>(() =>
                    {
                        try
                        {
                            return projectItem.Document;
                        }
                        catch (System.Runtime.InteropServices.COMException)
                        {
                            return null;
                        }
                    })();
                    if (document != null && document.Language == "CSharp")
                    {
                        solutionCodeDocuments.Add(
                            Tuple.Create<string, string>((string)projectItem.Properties.Item("FullPath").Value, projectItem.ContainingProject.Name));
                    }
                    else
                    {
                        //This is alternative approach for getting document...
                        try
                        {
                            var projectItemsFileCount = projectItem.FileCount;
                            for (int i = 0; i < projectItemsFileCount; i++)
                            {
                                try
                                {
                                    var test2 = projectItem.FileNames[(short)i];
                                    var test4 = projectItem.ContainingProject.Name;
                                    string extension = Path.GetExtension(test2);
                                    if (extension == ".cs")
                                    {
                                        solutionCodeDocuments.Add(Tuple.Create<string, string>(test2, test4));
                                    }
                                    else if (extension == ".xml")
                                    {
                                        solutionXmlDocument.Add(Tuple.Create<string, string>(test2, test4));
                                    }
                                    var test3 = projectItem.ContainingProject.CodeModel.Language;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                    System.Diagnostics.Debugger.Break();
                                }
                            }
                        }
                        catch(COMException comException)
                        {
                            Debug.WriteLine(comException.Message);
                            //System.Diagnostics.Debugger.Break();
                        }
                    }
                }
                CSharpIndexer csIndexer = new CSharpIndexer();
                XmlIndexer indexerInstance = new XmlIndexer();

                var testIndexerResult = indexerInstance.BuildIndexer(solutionXmlDocument, Path.GetFileNameWithoutExtension(_dte.Solution.FileName));
                Indexer.Build(testIndexerResult);
                //Indexer.BuildFromXml(testIndexerResult);

                var codeIndexerResult = await csIndexer.BuildIndexerAsync(solutionCodeDocuments, Path.GetFileNameWithoutExtension(_dte.Solution.FullName));
                //Indexer.AppendCSharpFileStatment(codeIndexerResult);
                Indexer.Build(codeIndexerResult);

            }
        }
        private SolutionEventsHandler _solutionEventsHandler;
        private TextManagerEvents _textManagerEvents;
        private TextManagerLineEvents _textManagerLineEvents;

        internal class TextManagerLineEvents : IVsTextLinesEvents
        {
            public void OnChangeLineText(TextLineChange[] pTextLineChange, int fLast)
            {
                throw new NotImplementedException();
            }

            public void OnChangeLineAttributes(int iFirstLine, int iLastLine)
            {
                throw new NotImplementedException();
            }
        }

        internal class TextManagerEvents : IVsTextManagerEvents2
        {
            public int OnRegisterMarkerType(int iMarkerType)
            {
                throw new NotImplementedException();
            }

            public int OnRegisterView(IVsTextView pView)
            {
                throw new NotImplementedException();
            }

            public int OnUnregisterView(IVsTextView pView)
            {
                throw new NotImplementedException();
            }

            public int OnUserPreferencesChanged2(VIEWPREFERENCES2[] pViewPrefs, FRAMEPREFERENCES2[] pFramePrefs, LANGPREFERENCES2[] pLangPrefs, FONTCOLORPREFERENCES2[] pColorPrefs)
            {
                throw new NotImplementedException();
            }

            public int OnReplaceAllInFilesBegin()
            {
                throw new NotImplementedException();
            }

            public int OnReplaceAllInFilesEnd()
            {
                throw new NotImplementedException();
            }
        }
        internal class SolutionEventsHandler : IVsSolutionLoadEvents, IVsSolutionEvents
        {
            private GotoPackage _package;
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
                _package.HandleSolutionEventAsync(1);
                return 0;
            }

            public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
            {
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
                var test = true;
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
