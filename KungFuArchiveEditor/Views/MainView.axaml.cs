using Avalonia.Controls;
using Avalonia.Interactivity;
using KungFuArchiveEditor.Tools;
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
            await GameMetaData.LoadAsync();
        }
        catch (Exception)
        {

        }
    }
}
