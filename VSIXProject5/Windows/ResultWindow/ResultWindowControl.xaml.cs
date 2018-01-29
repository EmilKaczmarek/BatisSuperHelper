namespace VSIXProject5
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using VSIXProject5.Indexers.Models;
    using VSIXProject5.VSIntegration.Navigation;

    /// <summary>
    /// Interaction logic for ResultWindowControl.
    /// </summary>
    public partial class ResultWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultWindowControl"/> class.
        /// </summary>
        public ResultWindowControl()
        {
            this.InitializeComponent();
            this.MouseDoubleClick += ResultWindowControl_MouseDoubleClick;
        }

        private void ResultWindowControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            var currentItem = dataGrid.CurrentItem as BaseIndexerValue;
            DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(currentItem.QueryFilePath, currentItem.QueryLineNumber);
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
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "ResultWindow");
        }
        public void ShowResults<T>(List<T> results) where T: BaseIndexerValue
        {
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = results;
        }


    }
}