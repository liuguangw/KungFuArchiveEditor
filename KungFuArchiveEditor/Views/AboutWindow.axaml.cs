using Avalonia.Controls;
using Avalonia.Interactivity;
using KungFuArchiveEditor.Assets;
using System.Diagnostics;

namespace KungFuArchiveEditor.Views;
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }
    private void OpenUrlHandler(object sender, RoutedEventArgs args)
    {
        var targetUrl = LangResources.GitHubUrl;
        var processStartInfo = new ProcessStartInfo(targetUrl)
        {
            UseShellExecute = true
        };
        Process.Start(processStartInfo);
    }
}
