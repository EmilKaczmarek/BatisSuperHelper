namespace VSIXProject5.Windows.RenameWindow
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.PlatformUI;

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
    [Guid("608962e5-a18a-4d7f-9b7f-d038a5bff74f")]
    public class RenameModalWindow : DialogWindow //ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameModalWindow"/> class.
        /// </summary>
        public RenameModalWindow() : base("wtfHelpTopic")
        {
            this.HasMaximizeButton = true;
            this.HasMinimizeButton = true;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            //this.Content = new RenameModalWindowControl();
        }

    }
}
