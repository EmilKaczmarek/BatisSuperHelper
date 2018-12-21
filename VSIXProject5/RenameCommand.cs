using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSIXProject5.Windows.RenameWindow;
using VSIXProject5.Windows.RenameWindow.ViewModel;
using VSIXProject5.Indexers;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using VSIXProject5.Constants;
using VSIXProject5.Helpers;
using VSIXProject5.Actions2;

namespace VSIXProject5
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RenameCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("5c26ab2a-b2b0-47f6-9eb7-6534d0b372f2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private BaseActions _commandActions;
        private OleMenuCommand menuItem;
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private RenameCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            //_commandActions = new QueryRenameActions(this.package as GotoAsyncPackage);

            //OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            //if (commandService != null)
            //{
            //    var menuCommandID = new CommandID(CommandSet, CommandId);
            //    var menuItem = new OleMenuCommand(_commandActions.MenuItemCallback,_commandActions.Change, _commandActions.BeforeQuery, menuCommandID);
            //    commandService.AddCommand(menuItem);
            //}
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RenameCommand Instance
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
            Instance = new RenameCommand(package);
        }
    }
}
