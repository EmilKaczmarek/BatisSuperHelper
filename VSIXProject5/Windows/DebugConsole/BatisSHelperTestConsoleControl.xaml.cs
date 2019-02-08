namespace IBatisSuperHelper
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using IBatisSuperHelper.Indexers;

    /// <summary>
    /// Interaction logic for BatisSHelperTestConsoleControl.
    /// </summary>
    public partial class BatisSHelperTestConsoleControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatisSHelperTestConsoleControl"/> class.
        /// </summary>
        public BatisSHelperTestConsoleControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //var copy = Indexer.statmentInfo;
            //var builder = new System.Text.StringBuilder();
            //builder.Append(textBox.Text);
            //foreach (var kv in copy)
            //{
            //    builder.Append(kv.Key.StatmentName + " " + kv.Key.VsProjectName + "\n");
            //    if (kv.Value.XmlInfo != null)
            //        builder.Append(kv.Value.XmlInfo.StatmentFile + " " + kv.Value.XmlInfo.LineNumer + " " + kv.Value.XmlInfo.RelativePath+ "\n");
            //    if (kv.Value.CodeInfo != null)
            //        builder.Append(kv.Value.CodeInfo.StatmentFile + " " + kv.Value.CodeInfo.LineNumber + " " + kv.Value.CodeInfo.RelativePath + "\n");

            //}
            //builder.AppendLine("");
            //builder.AppendLine("---------------------------------------------------");
            //textBox.Text = builder.ToString();

        }
    }
}