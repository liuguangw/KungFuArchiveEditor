using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace KungFuArchiveEditor.ViewModels;

public class BagItemEquipViewModel : BagItemViewModel
{
    private int rarity = 0;

    private JValue? rarityObject = null;
    public int Rarity
    {
        get => rarity;
        set => RaiseAndSetIfChanged(ref rarity, value, rarityObject);
    }
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
        //rarity
        var objectNode = jsonData["rarity"];
        if (objectNode != null)
        {
            rarity = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                rarityObject = value;
            }
        }
        if (jsonData["main_props"] is JArray propNodes)
        {
            foreach (var propNode in propNodes)
            {
                CheckAddPropNode(propNode, MainProps);
            }
        }
        if (jsonData["addon_props"] is JArray addonPropNodes)
        {
            foreach (var propNode in addonPropNodes)
            {
                CheckAddPropNode(propNode, AddonProps);
            }
        }
    }

    /// <summary>
    /// 解析属性类型和数值,并加入列表中
    /// </summary>
    /// <param name="propNode"></param>
    /// <param name="propCollections"></param>
    private static void CheckAddPropNode(JToken propNode, ObservableCollection<EquipPropViewModel> propCollections)
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
                    propCollections.Add(new EquipPropViewModel()
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
        GameConfigData.Equips.TryGetValue(classID, out string? equipName);
        if (equipName != null)
        {
            return equipName;
        }
        return "未知";
    }

    /// <summary>
    /// 更新装备属性的json对象字段,main_props和addon_props
    /// </summary>
    public void UpdatePropsJsonData()
    {
        if (itemJsonData == null)
        {
            return;
        }
        var propNames = new string[] { "main_props", "addon_props" };
        var propCollections = new ObservableCollection<EquipPropViewModel>[] { MainProps, AddonProps };
        for (var i = 0; i < propNames.Length; i++)
        {
            var propName = propNames[i];
            var propCollection = propCollections[i];
            if (itemJsonData[propName] is JArray propJsonNodes)
            {
                //清理
                propJsonNodes.Clear();
                //重新添加json对象
                foreach (var itemProp in propCollection)
                {
                    if (itemProp == null)
                    {
                        continue;
                    }
                    var propJsonNode = new JArray{
                        itemProp.Id,
                        itemProp.Value
                    };
                    propJsonNodes.Add(propJsonNode);
                }

            }
        }
    }
}
