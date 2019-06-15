using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BatisSuperHelper.Windows.RenameWindow
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RenameModalWindowCommand
    {
        public const int CommandId = 259;
        public static readonly Guid CommandSet = new Guid("16d16112-fe5d-40b1-9e67-9ae342523f8b");

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var cmd = new MenuCommand((s, e) => ShowToolWindow(package), menuCommandID);
            commandService.AddCommand(cmd);
        }

        private static void ShowToolWindow(AsyncPackage package)
        {
            ToolWindowPane window = package.FindToolWindow(typeof(RenameModalWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
