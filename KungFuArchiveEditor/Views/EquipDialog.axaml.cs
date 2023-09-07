using Avalonia.Controls;
using Avalonia.Interactivity;
using KungFuArchiveEditor.ViewModels;

namespace KungFuArchiveEditor.Views
{
    public partial class EquipDialog : Window
    {
        public EquipDialog()
        {
            InitializeComponent();
        }
        private void ConfirmHandler(object sender, RoutedEventArgs args)
        {
            if (DataContext is EquipDialogViewModel vm)
            {
                vm.Confirmed = true;
            }
            Close();
        }

        private void CancelHandler(object sender, RoutedEventArgs args) => Close();
    }
}
