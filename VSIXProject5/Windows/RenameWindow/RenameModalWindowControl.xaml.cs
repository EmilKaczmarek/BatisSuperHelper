namespace VSIXProject5.Windows.RenameWindow
{
    using Microsoft.VisualStudio.PlatformUI;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using VSIXProject5.Windows.RenameWindow.ViewModel;

    /// <summary>
    /// Interaction logic for RenameModalWindowControl.
    /// </summary>
    public partial class RenameModalWindowControl : DialogWindow
    {
        private RenameViewModel _renameViewModel;
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameModalWindowControl"/> class.
        /// </summary>
        public RenameModalWindowControl()
        {
            this.InitializeComponent();
            _renameViewModel = new RenameViewModel
            {
                QueryText = "bindTestQuery"
            };
            this.DataContext = _renameViewModel;
        }

        public RenameModalWindowControl(RenameViewModel viewModel)
        {
            this.InitializeComponent();
            _renameViewModel = viewModel;
            this.DataContext = _renameViewModel;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var text = this.QueryTextBox.Text;
            _renameViewModel.WasInputCanceled = false;
            //Rename Logic
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _renameViewModel.WasInputCanceled = true;
            this.Close();
        }

        private void DialogWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        CancelButton_Click(sender, null);
                        break;
                    }
                case Key.Enter:
                    {
                        RenameButton_Click(sender, null);
                        break;
                    }
            }
        }
    }
}