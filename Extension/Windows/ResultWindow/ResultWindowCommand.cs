using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BatisSuperHelper
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ResultWindowCommand
    {
        public const int CommandId = 258;
        public static readonly Guid CommandSet = new Guid("16d16112-fe5d-40b1-9e67-9ae342523f8b");

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ResultWindowCommand Instance
        {
            get;
            private set;
        }


        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var cmd = new MenuCommand((s, e) => ShowToolWindow(package), menuCommandID);
            commandService.AddCommand(cmd);
        }

        private static void ShowToolWindow(AsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ToolWindowPane window = package.FindToolWindow(typeof(ResultWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
