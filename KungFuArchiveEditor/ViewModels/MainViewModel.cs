using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using KungFuArchiveEditor.Assets;
using KungFuArchiveEditor.Views;
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

    public RoleViewModel RoleVm { get; } = new();
    public BagViewModel BagVm { get; } = new();

    public async void SaveFileAction()
    {
        if (currentJsonFile == null || worldJsonData == null)
        {
            return;
        }
        try
        {
            await SaveFileAsync(currentJsonFile, worldJsonData);
        }
        catch (Exception ex)
        {
            await ShowMessageTipAsync(tipViewModel =>
            {
                tipViewModel.Title = "保存存档文件出错";
                tipViewModel.Message = ex.Message;
                tipViewModel.TextColorHex = "#fc5531";
            });
        }
    }
    public async void OpenFileAction()
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow;
            var storageProvider = topWindow!.StorageProvider;
            try
            {
                await DoOpenFileAsync(storageProvider);
            }
            catch (Exception ex)
            {
                await ShowMessageTipAsync(tipViewModel =>
                {
                    tipViewModel.Title = "读取存档文件出错";
                    tipViewModel.Message = ex.Message;
                    tipViewModel.TextColorHex = "#fc5531";
                });
            }
        }
    }

    private async Task DoOpenFileAsync(IStorageProvider storageProvider)
    {
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
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
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

    private static async Task<JObject> LoadFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var encoding = Encoding.GetEncoding("utf-16");
        using var streamReader = new StreamReader(stream, encoding);
        var fileContent = await streamReader.ReadToEndAsync();
        return JObject.Parse(fileContent);
    }

    private static async Task SaveFileAsync(IStorageFile file, JObject jsonData)
    {
        var jsonContent = jsonData.ToString(Formatting.None);
        await using var stream = await file.OpenWriteAsync();
        var encoding = Encoding.GetEncoding("utf-16");
        using var streamWriter = new StreamWriter(stream, encoding);
        await streamWriter.WriteAsync(jsonContent);
    }

    private void LoadModelData(JObject jsonData)
    {
        //uid
        var playerUidObject = jsonData.SelectToken("player_system.cur_select_player_uid");
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
        var playerData = jsonData.SelectToken($"common_players_data.{playerUid}");
        if (playerData == null)
        {
            return;
        }
        RoleVm.LoadPlayerData(playerData);
        //物品列表
        var containerEntityMap = playerData.SelectToken("component_data.container.entity_map");
        if (containerEntityMap == null)
        {
            return;
        }
        if (containerEntityMap is JObject entryMap)
        {
            BagVm.LoadBagItemList(entryMap);
        }
    }

    public static async Task ShowMessageTipAsync(Action<MessageTipViewModel> action)
    {
        var dialogVm = new MessageTipViewModel();
        action.Invoke(dialogVm);
        var dialog = new MessageTipDialog
        {
            DataContext = dialogVm
        };
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow!;
            await dialog.ShowDialog<MessageTipViewModel?>(topWindow);
        }
    }
}
