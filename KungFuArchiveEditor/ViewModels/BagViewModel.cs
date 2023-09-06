using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using KungFuArchiveEditor.Views;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.ViewModels;
public class BagViewModel : ViewModelBase
{
    #region Properties
    public ObservableCollection<BagItemViewModel> BagItemList { get; } = new();
    public ReactiveCommand<BagItemViewModel, Unit> EditEquipCommand { get; }
    #endregion
    public BagViewModel()
    {
        EditEquipCommand = ReactiveCommand.Create<BagItemViewModel>(EditEquipAction);
    }
    /// <summary>
    /// 从json对象中加载物品列表
    /// </summary>
    /// <param name="entityMap"></param>
    public void LoadBagItemList(JObject entityMap)
    {
        BagItemList.Clear();
        var itemNodes = entityMap.Properties()
            .Where(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                var mainPos = posArr[0];
                return mainPos == 2 || mainPos == 4;
            })
            //道具在前面
            .OrderByDescending(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                return posArr[0];
            })
            //按位置排序
            .ThenBy(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                return posArr[2];
            });
        //
        foreach (var itemNodeProperty in itemNodes)
        {
            if (itemNodeProperty == null)
            {
                continue;
            }
            BagItemList.Add(ParseBagItem(itemNodeProperty.Name, itemNodeProperty.Value));
        }
    }

    private static int[] ParsePos(string posKey)
    {
        var posArr = new int[] { 0, 0, 0 };
        //解析pos
        var numTexts = posKey.Split('-');
        if (numTexts.Length == 3)
        {
            posArr[0] = int.Parse(numTexts[0]);
            posArr[1] = int.Parse(numTexts[1]);
            posArr[2] = int.Parse(numTexts[2]);
        }

        return posArr;
    }

    private static BagItemViewModel ParseBagItem(string posKey, JToken itemJsonData)
    {
        var entityType = 0;
        var objectNode = itemJsonData["entity_type"];
        if (objectNode != null)
        {
            entityType = objectNode.ToObject<int>();
        }
        //
        var posArr = ParsePos(posKey);
        BagItemViewModel bagItem;
        if (entityType == 5)
        {
            //装备
            bagItem = new BagItemEquipViewModel();
        }
        else
        {
            bagItem = new BagItemViewModel();
        }
        bagItem.LoadItemData(posArr, entityType, itemJsonData);
        return bagItem;
    }

    public async void EditEquipAction(BagItemViewModel bagItem)
    {
        if (bagItem is not BagItemEquipViewModel)
        {
            return;
        }
        var equipItem = bagItem as BagItemEquipViewModel;
        if (equipItem == null)
        {
            return;
        }
        var dialogVm = await ShowEquipDialogAsync(vm =>
        {
            vm.ClassID = equipItem.ClassID;
            vm.Rarity = equipItem.Rarity;
            vm.PosKey = equipItem.PosKey;
            //创建复制的对象,防止影响
            foreach (var mainProp in equipItem.MainProps)
            {
                vm.MainProps.Add(new EquipPropViewModel()
                {
                    IsMainProp = true,
                    Id = mainProp.Id,
                    Value = mainProp.Value,
                });
            }
            foreach (var addonProp in equipItem.AddonProps)
            {
                vm.AddonProps.Add(new EquipPropViewModel()
                {
                    Id = addonProp.Id,
                    Value = addonProp.Value,
                });
            }
        });
        //确认修改之后,再复制到装备对象
        if (dialogVm.Confirmed)
        {
            equipItem.Rarity = dialogVm.Rarity;
            equipItem.MainProps.Clear();
            equipItem.AddonProps.Clear();
            foreach (var mainProp in dialogVm.MainProps)
            {
                equipItem.MainProps.Add(mainProp);
            }
            foreach (var addonProp in dialogVm.AddonProps)
            {
                equipItem.AddonProps.Add(addonProp);
            }
            equipItem.UpdatePropsJsonData();
        }
    }

    private static async Task<EquipDialogViewModel> ShowEquipDialogAsync(Action<EquipDialogViewModel> action)
    {
        var dialogVm = new EquipDialogViewModel();
        action.Invoke(dialogVm);
        var dialog = new EquipDialog()
        {
            DataContext = dialogVm
        };
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow!;
            await dialog.ShowDialog<MessageTipViewModel?>(topWindow);
        }
        return dialogVm;
    }
}
