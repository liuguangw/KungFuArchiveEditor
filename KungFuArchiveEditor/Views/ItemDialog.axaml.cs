using Avalonia.Controls;
using Avalonia.Interactivity;
using KungFuArchiveEditor.ViewModels;

namespace KungFuArchiveEditor.Views;
public partial class ItemDialog : Window
{
    public ItemDialog()
    {
        InitializeComponent();
    }
    private void ConfirmHandler(object sender, RoutedEventArgs args)
    {
        if (DataContext is ItemDialogViewModel vm)
        {
            vm.Confirmed = true;
        }
        Close();
    }

    private void CancelHandler(object sender, RoutedEventArgs args) => Close();
}
