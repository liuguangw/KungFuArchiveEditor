using Avalonia.Controls;
using Avalonia.Interactivity;
using KungFuArchiveEditor.Tools;
using KungFuArchiveEditor.ViewModels;
using System;

namespace KungFuArchiveEditor.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        Loaded += MainView_Loaded;
    }

    private async void MainView_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await GameConfigData.LoadAsync();
        }
        catch (Exception ex)
        {
            await MainViewModel.ShowMessageTipAsync(tipViewModel =>
            {
                tipViewModel.Title = "出错了";
                tipViewModel.Message = ex.Message;
                tipViewModel.TextColorHex = "#fc5531";
            });
        }
    }
}
