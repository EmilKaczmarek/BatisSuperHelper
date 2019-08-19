using BatisSuperHelper.Actions;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper
{
    public class PrettyPrintCommand
    {
        public const int CommandId = 257;
        public static readonly Guid CommandSet = new Guid("EA0E0C33-3583-460F-BEA0-89BA2AAA4CAD");

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            var handler = new PrettyPrintActions(package as GotoAsyncPackage);

            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(handler.MenuItemCallback, handler.Change, handler.BeforeQuery, menuCommandID);

            commandService.AddCommand(menuItem);
        }
    }
}
