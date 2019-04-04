namespace IBatisSuperHelper
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using IBatisSuperHelper.Indexers.Models;
    using IBatisSuperHelper.VSIntegration.Navigation;
    using IBatisSuperHelper.Windows.ResultWindow.ViewModel;


    /// <summary>
    /// Interaction logic for ResultWindowControl.
    /// </summary>
    public partial class ResultWindowControl : UserControl
    {
        private static double _lastColumnInitialWidth;
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultWindowControl"/> class.
        /// </summary>
        public ResultWindowControl()
        {
            this.InitializeComponent();

            _lastColumnInitialWidth = (listView.View as GridView).Columns[(listView.View as GridView).Columns.Count - 1].Width;
        }

        public void ShowResults(List<ResultWindowViewModel> resultWindowsViewModels)
        {
            this.listView.ItemsSource = resultWindowsViewModels;
        }

        private void listView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeLastColumn(sender as ListView);
        }

        private void listView_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeLastColumn(sender as ListView);
        }

        private void ResizeLastColumn(ListView listView)
        {
            var gridView = listView.View as GridView;
            double columnsWidthWithoutLast = 0;
            for(int i = 0; i < gridView.Columns.Count-1; i++)
            {
                columnsWidthWithoutLast += gridView.Columns[i].ActualWidth;
            }

            var lastColumn = gridView.Columns[gridView.Columns.Count-1];

            lastColumn.Width = _lastColumnInitialWidth;
            var lastColumnTargetWidth = listView.ActualWidth - columnsWidthWithoutLast;
            if(lastColumn.Width < lastColumnTargetWidth)
            {
                lastColumn.Width = lastColumnTargetWidth;
            }
            
        }

        private void listView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigateToSelected(sender);
        }

        private void listView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                NavigateToSelected(sender);
            }
        }

        private void NavigateToSelected(object sender)
        {
            var selectedItem = (sender as ListView).SelectedItem as ResultWindowViewModel;
            if (selectedItem != null)
            {
                DocumentNavigationInstance.instance.OpenDocumentAndHighlightLine(selectedItem.FilePath, selectedItem.Line);
            }
        }
    }
}