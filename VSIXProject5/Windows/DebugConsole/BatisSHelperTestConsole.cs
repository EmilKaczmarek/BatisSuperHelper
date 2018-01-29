namespace VSIXProject5
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("d2dd489e-6fbd-4399-8c68-8c0568ec4e00")]
    public class BatisSHelperTestConsole : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatisSHelperTestConsole"/> class.
        /// </summary>
        public BatisSHelperTestConsole() : base(null)
        {
            this.Caption = "BatisSHelperTestConsole";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new BatisSHelperTestConsoleControl();
        }
    }
}
