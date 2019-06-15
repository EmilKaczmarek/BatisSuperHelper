//------------------------------------------------------------------------------
// <copyright file="Goto.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using BatisSuperHelper.Actions;

namespace BatisSuperHelper
{
    sealed class Goto
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("74b835d6-70a4-4629-9d2c-520ce16236b5");

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            var handler = new GoToQueryActions2(package as GotoAsyncPackage);

            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(
                   new EventHandler(handler.MenuItemCallback),
                   new EventHandler(handler.Change),
                   new EventHandler(handler.BeforeQuery),
                   menuCommandID,
                   "Go to Query");

            commandService.AddCommand(menuItem);
        }
    }
}