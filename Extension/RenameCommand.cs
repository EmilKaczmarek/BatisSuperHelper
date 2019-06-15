using System;
using Microsoft.VisualStudio.Shell;
using BatisSuperHelper.Actions;
using System.ComponentModel.Design;

namespace BatisSuperHelper
{
    internal sealed class RenameCommand
    {
        public const int CommandId = 256;
        public static readonly Guid CommandSet = new Guid("5c26ab2a-b2b0-47f6-9eb7-6534d0b372f2");

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            var handler = new QueryRenameActions(package as GotoAsyncPackage);

            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(handler.MenuItemCallback, handler.Change, handler.BeforeQuery, menuCommandID);

            commandService.AddCommand(menuItem);
        }
    }
}
