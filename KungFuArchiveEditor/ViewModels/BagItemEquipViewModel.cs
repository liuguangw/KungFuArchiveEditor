using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;

namespace KungFuArchiveEditor.ViewModels;

public class BagItemEquipViewModel : BagItemViewModel
{
    public ObservableCollection<EquipPropViewModel> MainProps { get; } = new();
    public ObservableCollection<EquipPropViewModel> AddonProps { get; } = new();

    /// <summary>
    /// 从json对象中读取装备属性
    /// </summary>
    /// <param name="posArr">位置(3个数字)</param>
    /// <param name="itemEntityType">实体类型</param>
    /// <param name="jsonData">json对象</param>
    public override void LoadItemData(int[] posArr, int itemEntityType, JToken jsonData)
    {
        base.LoadItemData(posArr, itemEntityType, jsonData);
        if (jsonData["main_props"] is JArray propNodes)
        {
            foreach (var propNode in propNodes)
            {
                CheckAddPropNode(propNode, MainProps.Add);
            }
        }
        if (jsonData["addon_props"] is JArray addonPropNodes)
        {
            foreach (var propNode in addonPropNodes)
            {
                CheckAddPropNode(propNode, AddonProps.Add);
            }
        }
    }

    /// <summary>
    /// 解析属性类型和数值,并加入列表中
    /// </summary>
    /// <param name="propNode"></param>
    /// <param name="addAction"></param>
    private static void CheckAddPropNode(JToken propNode, Action<EquipPropViewModel> addAction)
    {
        if (propNode is JArray numNodes)
        {
            if (numNodes.Count == 2)
            {
                var firstNode = numNodes[0];
                var secondNode = numNodes[1];
                if (firstNode != null && secondNode != null)
                {
                    var propID = firstNode.ToObject<int>();
                    var propValue = secondNode.ToObject<int>();
                    addAction.Invoke(new EquipPropViewModel()
                    {
                        Id = propID,
                        Value = propValue
                    });
                }
            }
        }

    }

    public override string GetItemName(int classID)
    {
        return GameMetaData.GetItemName(classID, GameMetaData.MetaType.Equip) ?? "未知";
    }
}