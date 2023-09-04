using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace KungFuArchiveEditor.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RoleViewModel RoleVm { get; } = new();
    public string Greeting => "Welcome to Avalonia!";
    private async Task OpenFileAction()
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow;
            var files = await topWindow!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Json File",
                AllowMultiple = false
            });
            if (files.Count >= 1)
            {
                await LoadFileAsync(files[0]);
            }
        }
    }

    private async Task LoadFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var encoding = Encoding.GetEncoding("utf-16");
        using var streamReader = new StreamReader(stream, encoding);
        var fileContent = await streamReader.ReadToEndAsync();
    }
}
