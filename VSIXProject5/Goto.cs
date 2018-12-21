//------------------------------------------------------------------------------
// <copyright file="Goto.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using VSIXProject5.Actions2;

namespace VSIXProject5
{
    /// <summary>
    /// Command handler
    /// </summary>
    sealed class Goto
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("74b835d6-70a4-4629-9d2c-520ce16236b5");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        public readonly Package package;

        private GoToQueryActions2 _commandActions;
        private OleMenuCommand menuItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="Goto"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Goto(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
            
            this.package = package;
 
            _commandActions = new GoToQueryActions2(this.package as GotoAsyncPackage);

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                menuItem = new OleMenuCommand(
                    new EventHandler(this._commandActions.MenuItemCallback),
                    new EventHandler(this._commandActions.Change),
                    new EventHandler(this._commandActions.BeforeQuery),
                    menuCommandID,
                    "Go to Query");

                commandService.AddCommand(menuItem);
            }                     
        }
       
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Goto Instance
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
            Instance = new Goto(package);
        }
    }
}