using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using KungFuArchiveEditor.Assets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private JObject? worldJsonData;
    private IStorageFile? currentJsonFile;
    public string Greeting => "Welcome to Avalonia!";
    public RoleViewModel RoleVm { get; } = new();

    public async void SaveFileAction()
    {
        if (currentJsonFile == null || worldJsonData == null)
        {
            return;
        }
        await SaveFileAsync(currentJsonFile, worldJsonData);
    }
    public async void OpenFileAction()
    {
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow;
            var storageProvider = topWindow!.StorageProvider;
            var fileTypeFilter = new FilePickerFileType(LangResources.ArchiveFile)
            {
                Patterns = new[] { "share_world_data.json" },
                MimeTypes = new[] { "application/json" }
            };
            var openOptions = new FilePickerOpenOptions
            {
                Title = LangResources.OpenArchiveFile,
                FileTypeFilter = new[] { fileTypeFilter },
                AllowMultiple = false
            };
            if (appDataDir != null)
            {
                var pathDirs = new[]
                {
                    appDataDir,
                    "HMS_00","Saved","PersistentDownloadDir","Saves"
                };
                var jsonDir = Path.Combine(pathDirs);
                openOptions.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(jsonDir);
            }
            var files = await storageProvider.OpenFilePickerAsync(openOptions);
            if (files.Count >= 1)
            {
                worldJsonData = await LoadFileAsync(files[0]);
                if (worldJsonData != null)
                {
                    LoadModelData(worldJsonData);
                    currentJsonFile = files[0];
                }
            }
        }
    }

    private static async Task<JObject> LoadFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var encoding = Encoding.GetEncoding("utf-16");
        using var streamReader = new StreamReader(stream, encoding);
        var fileContent = await streamReader.ReadToEndAsync();
        return JObject.Parse(fileContent);
    }

    private static async Task SaveFileAsync(IStorageFile file,JObject jsonData)
    {
        var jsonContent = jsonData.ToString(Formatting.None);
        await using var stream = await file.OpenWriteAsync();
        var encoding = Encoding.GetEncoding("utf-16");
        using var streamWriter = new StreamWriter(stream, encoding);
        await streamWriter.WriteAsync(jsonContent);
    }

    private void LoadModelData(JObject jsonData)
    {
        var playerSystem = jsonData["player_system"];
        if (playerSystem == null)
        {
            return;
        }
        //uid
        var playerUidObject = playerSystem["cur_select_player_uid"];
        if (playerUidObject == null)
        {
            return;
        }
        var playerUid = playerUidObject.ToObject<string>();
        if (playerUid == null)
        {
            return;
        }
        RoleVm.UserID = playerUid;
        //获取玩家对象
        var commonPlayersData = jsonData["common_players_data"];
        if (commonPlayersData == null)
        {
            return;
        }
        var playerData = commonPlayersData[playerUid];
        if (playerData == null)
        {
            return;
        }
        RoleVm.LoadPlayerData(playerData);
    }
}
