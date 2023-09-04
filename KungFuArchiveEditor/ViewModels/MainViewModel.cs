using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using KungFuArchiveEditor.Assets;

namespace KungFuArchiveEditor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private JObject? worldJsonData;
    public string Greeting => "Welcome to Avalonia!";
    public RoleViewModel RoleVm { get; } = new();
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
                }
            }
        }
    }

    private async Task<JObject> LoadFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var encoding = Encoding.GetEncoding("utf-16");
        using var streamReader = new StreamReader(stream, encoding);
        var fileContent = await streamReader.ReadToEndAsync();
        return JObject.Parse(fileContent);
    }

    private void LoadModelData(JObject jsonData)
    {
        var playerSystem = jsonData["player_system"];
        if (playerSystem == null)
        {
            return;
        }
        //uid
        string? playerUid = null;
        ReadJsonValue(playerSystem, "cur_select_player_uid", ref playerUid);
        if (playerUid != null)
        {
            RoleVm.UserID = playerUid;
        }
        else
        {
            return;
        }
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
        //name
        string? name = null;
        ReadJsonValue(playerData, "name", ref name);
        if (name != null)
        {
            RoleVm.Name = name;
        }
        //max hp
        int? maxHP = null;
        ReadJsonValue(playerData, "max_hp", ref maxHP);
        if (maxHP.HasValue)
        {
            RoleVm.MaxHP = maxHP.Value;
        }
        //max zhenqi
        int? maxZhenqi = null;
        ReadJsonValue(playerData, "max_zhenqi", ref maxZhenqi);
        if (maxZhenqi.HasValue)
        {
            RoleVm.MaxZhenqi = maxZhenqi.Value;
        }
        //max burden
        int? maxBurden = null;
        ReadJsonValue(playerData, "max_burden", ref maxBurden);
        if (maxBurden.HasValue)
        {
            RoleVm.MaxBurden = maxBurden.Value;
        }
    }

    private void ReadJsonValue<T>(JToken token, string key, ref T? value)
    {
        var nodeObject = token[key];
        if (nodeObject == null)
        {
            return;
        }
        value = nodeObject.ToObject<T>();
    }
}
