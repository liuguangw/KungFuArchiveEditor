using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using KungFuArchiveEditor.Assets;
using KungFuArchiveEditor.Tools;
using KungFuArchiveEditor.Views;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private JObject? worldJsonData;
    private IStorageFile? currentJsonFile;
    private IStorageFolder? lastOpenFolder;

    public RoleViewModel RoleVm { get; } = new();
    public BagViewModel BagVm { get; } = new();
    public AbilityViewModel AbilityVm { get; } = new();
    public JingmaiViewModel JingmaiVm { get; } = new();

    public async void SaveFileAction()
    {
        if (currentJsonFile == null || worldJsonData == null)
        {
            return;
        }
        try
        {
            await ArchiveTool.SaveArchiveAsync(currentJsonFile, worldJsonData);
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
    /// <summary>
    /// 关于
    /// </summary>
    public async void AboutAction()
    {
        await ShowAboutWindowAsync();
    }

    /// <summary>
    /// 退出
    /// </summary>
    public void ExitAction()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.Shutdown();
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
            Patterns = new[] { "share_world_data.json", "share_world_data.data" },
            MimeTypes = new[] { "application/json", "application/octet-stream" }
        };
        var openOptions = new FilePickerOpenOptions
        {
            Title = LangResources.OpenArchiveFile,
            FileTypeFilter = new[] { fileTypeFilter },
            AllowMultiple = false
        };
        await SetSuggestedFolderAsync(storageProvider, openOptions);
        var files = await storageProvider.OpenFilePickerAsync(openOptions);
        if (files.Count >= 1)
        {
            //记录最后打开的文件夹
            lastOpenFolder = await files[0].GetParentAsync();
            //加载存档文件内容
            worldJsonData = await ArchiveTool.LoadArchiveAsync(files[0]);
            if (worldJsonData != null)
            {

                LoadModelData(worldJsonData);
                currentJsonFile = files[0];
                InitUidTool(worldJsonData, lastOpenFolder!);
            }
        }
    }

    private static async void InitUidTool(JObject shareWorldData, IStorageFolder saveFolder)
    {
        //在后台执行
        await Task.Run(async () =>
        {
            await UidTool.InitToolAsync(shareWorldData, saveFolder);
        });
    }

    /// <summary>
    /// 选择文件时,设置默认打开的文件夹
    /// </summary>
    /// <param name="storageProvider"></param>
    /// <param name="openOptions"></param>
    private async Task SetSuggestedFolderAsync(IStorageProvider storageProvider, FilePickerOpenOptions openOptions)
    {
        if (lastOpenFolder != null)
        {
            openOptions.SuggestedStartLocation = lastOpenFolder;
            return;
        }
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (appDataDir != null)
        {
            var pathDirs = new[]
            {
                    appDataDir,
                    "HMS_00","Saved","PersistentDownloadDir"
                };
            var saveDir = Path.Combine(pathDirs);
            openOptions.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(saveDir);
        }
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
            BagVm.LoadBagItemList(playerUid, entryMap);
        }
        //能力列表
        var abilityEntityMap = playerData.SelectToken("component_data.ability_container.entity_map");
        if (abilityEntityMap == null)
        {
            return;
        }
        if (abilityEntityMap is JObject abilityMap)
        {
            AbilityVm.LoadItemList(abilityMap);
        }
        //经脉
        var jingmaiData = playerData.SelectToken("component_data.jingmai");
        if (jingmaiData == null)
        {
            return;
        }
        if (jingmaiData is JObject jingmaiDataObj)
        {
            JingmaiVm.LoadJingmaiData(jingmaiDataObj);
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
            await dialog.ShowDialog(topWindow);
        }
    }

    private static async Task ShowAboutWindowAsync()
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow!;
            var dialog = new AboutWindow();
            await dialog.ShowDialog(topWindow);
        }
    }
}
