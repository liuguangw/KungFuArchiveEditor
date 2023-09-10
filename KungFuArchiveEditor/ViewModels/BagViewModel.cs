using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using KungFuArchiveEditor.Tools;
using KungFuArchiveEditor.Views;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.ViewModels;
public class BagViewModel : ViewModelBase
{
    private JObject? itemListData = null;
    private string playerUid = string.Empty;
    #region Properties
    public ObservableCollection<BagItemViewModel> BagItemList { get; } = new();
    public ReactiveCommand<BagItemViewModel, Unit> EditEquipCommand { get; }
    public ReactiveCommand<Unit, Unit> AddItemCommand { get; }
    public bool ItemListLoaded => itemListData != null;
    #endregion
    public BagViewModel()
    {
        EditEquipCommand = ReactiveCommand.Create<BagItemViewModel>(EditEquipAction);
        var canAdd = this.WhenAnyValue(item => item.ItemListLoaded);
        AddItemCommand = ReactiveCommand.Create(AddItemAction, canAdd);
    }
    /// <summary>
    /// 从json对象中加载物品列表
    /// </summary>
    /// <param name="uid">用户uid</param>
    /// <param name="entityMap"></param>
    public void LoadBagItemList(string uid, JObject entityMap)
    {
        playerUid = uid;
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
            /*.ThenBy(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                return posArr[2];
            })*/;
        //
        foreach (var itemNodeProperty in itemNodes)
        {
            if (itemNodeProperty == null)
            {
                continue;
            }
            BagItemList.Add(ParseBagItem(itemNodeProperty.Name, itemNodeProperty.Value));
        }
        var loaded = ItemListLoaded;
        itemListData = entityMap;
        if (!loaded)
        {
            this.RaisePropertyChanged(nameof(ItemListLoaded));
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

    public async void AddItemAction()
    {
        var dialogVm = await ShowItemDialogAsync(vm => vm.ClassID = 80001);
        if (dialogVm.Confirmed)
        {
            Debug.WriteLine($"class_id={dialogVm.ClassID} name={dialogVm.Name} amount={dialogVm.Amount}/{dialogVm.MaxAmount}");
            Debug.WriteLine($"is_item={dialogVm.IsItem}, is_equip={dialogVm.IsEquip}");
            try
            {
                if (dialogVm.IsItem)
                {
                    ProcessAddItem(dialogVm);
                }
                await MainViewModel.ShowMessageTipAsync(vm =>
                {
                    vm.Title = "发放成功";
                    vm.Message = $"发放{dialogVm.Name} * {dialogVm.Amount}成功";
                    vm.TextColor = Brushes.Green;
                });
            }
            catch (Exception ex)
            {
                await MainViewModel.ShowMessageTipAsync(vm =>
                {
                    vm.Title = "出错了";
                    vm.Message = ex.Message;
                    vm.TextColor = Brushes.Red;
                });
            }
        }
    }

    private void ProcessAddItem(ItemDialogViewModel dialogVm)
    {
        var classID = dialogVm.ClassID;
        var amount = dialogVm.Amount;
        if (dialogVm.MaxAmount > 1)
        {
            var incrAmount = ProcessIncrItemAmount(classID, amount, dialogVm.MaxAmount);
            amount -= incrAmount;
            if (amount == 0)
            {
                //添加成功
                return;
            }
        }
        //添加新记录
        ProcessAddNewItem(classID, amount);
    }

    /// <summary>
    /// 增加已有物品的数量
    /// </summary>
    /// <param name="classID"></param>
    /// <param name="amount"></param>
    /// <param name="maxAmount"></param>
    /// <returns>成功增加的数量</returns>
    /// <exception cref="NotImplementedException"></exception>
    private int ProcessIncrItemAmount(int classID, int amount, int maxAmount)
    {
        foreach (var item in BagItemList)
        {
            if (item.ClassID == classID)
            {
                var itemCurrentAmount = item.Amount;
                var incrAmount = Math.Min(amount, maxAmount - itemCurrentAmount);
                item.Amount += incrAmount;
                return incrAmount;
            }
        }
        return 0;
    }

    /// <summary>
    /// 添加新的物品记录
    /// </summary>
    /// <param name="classID"></param>
    /// <param name="amount"></param>
    private void ProcessAddNewItem(int classID, int amount)
    {
        var item = new BagItemViewModel();
        var mainPos = 4;
        var subPos = 0;
        var pos = 0;
        var insertIndex = 0;
        for (var i = 0; i < BagItemList.Count; i++)
        {
            var bagItem = BagItemList[i];
            if (bagItem.MainPos == mainPos && bagItem.SubPos == subPos)
            {
                insertIndex = i + 1;
                if (pos <= bagItem.Pos)
                {
                    pos = bagItem.Pos + 1;
                }
            }
        }
        long itemUid = UidTool.NextUid();
        var posArr = new int[] { mainPos, subPos, pos };
        var jsonData = item.InitNewJson(posArr, classID, amount, itemUid.ToString(), playerUid);
        if (itemListData != null)
        {
            itemListData.Add(item.PosKey, jsonData);
            //判断界面上的新物品行应该放到哪里
            if (insertIndex < BagItemList.Count)
            {
                BagItemList.Insert(insertIndex, item);
            }
            else
            {
                BagItemList.Add(item);
            }
        }
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
                    //IsMainProp = true,
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
            await dialog.ShowDialog(topWindow);
        }
        return dialogVm;
    }
    /// <summary>
    /// 显示发放物品按钮
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private static async Task<ItemDialogViewModel> ShowItemDialogAsync(Action<ItemDialogViewModel> action)
    {
        var dialogVm = new ItemDialogViewModel();
        action.Invoke(dialogVm);
        var dialog = new ItemDialog()
        {
            DataContext = dialogVm
        };
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topWindow = desktop.MainWindow!;
            await dialog.ShowDialog(topWindow);
        }
        return dialogVm;
    }
}
