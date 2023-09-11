using KungFuArchiveEditor.GameConfig;
using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace KungFuArchiveEditor.ViewModels;

public class BagItemEquipViewModel : BagItemViewModel
{
    private int rarity = 0;

    private JValue? rarityObject = null;
    private JArray? mainPropsObject = null;
    private JArray? addonPropsObject = null;
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
            mainPropsObject = propNodes;
            foreach (var propNode in propNodes)
            {
                CheckAddPropNode(propNode, MainProps);
            }
        }
        if (jsonData["addon_props"] is JArray addonPropNodes)
        {
            addonPropsObject = addonPropNodes;
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
        GameConfigData.Equips.TryGetValue(classID, out EquipConfig? equipConfig);
        if (equipConfig != null)
        {
            return equipConfig.Name;
        }
        return "未知";
    }

    /// <summary>
    /// 更新装备属性的json对象字段,main_props和addon_props
    /// </summary>
    public void UpdatePropsJsonData()
    {
        if (mainPropsObject == null || addonPropsObject == null)
        {
            return;
        }
        var propObjects = new JArray[] { mainPropsObject, addonPropsObject };
        var propCollections = new ObservableCollection<EquipPropViewModel>[] { MainProps, AddonProps };
        for (var i = 0; i < propObjects.Length; i++)
        {
            var propJsonNodes = propObjects[i];
            //清理
            propJsonNodes.Clear();
            //重新添加json对象
            var propCollection = propCollections[i];
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

    /// <summary>
    /// 初始化新的装备对象
    /// </summary>
    /// <param name="posArr"></param>
    /// <param name="classID"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public override void InitItem(int[] posArr, int classID, int amount = 1)
    {
        mainPos = posArr[0];
        subPos = posArr[1];
        pos = posArr[2];
        entityType = 5;
        this.classID = classID;
        this.amount = amount;
        rarity = 0;
        rarityObject = new JValue(rarity);
        mainPropsObject = new JArray();
        addonPropsObject = new JArray();
        if (GameConfigData.Equips.TryGetValue(classID, out var equipConfig))
        {
            var equipType = equipConfig.EquipType;
            int mainPropID = 0;
            int mainPropValue = 0;
            //不同的装备类型对应不同的主属性
            foreach (var propPair in GameConfigData.EquipMainProps)
            {
                if (propPair.Value.EquipType == equipType)
                {
                    mainPropID = propPair.Key;
                }
            }
            //默认的主属性值
            var propValueArr = new int[]
            {
                2,-2,200,5,3
            };
            if (equipType < propValueArr.Length)
            {
                mainPropValue = propValueArr[equipType];
            }
            MainProps.Add(new EquipPropViewModel()
            {
                Id = mainPropID,
                Value = mainPropValue
            });
        }
    }
    /// <summary>
    /// 转换为新的json对象
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="ownerUid"></param>
    /// <returns></returns>
    public override JObject ToNewJsonData(string uid, string ownerUid)
    {
        var jsonData = new JObject
        {
            ["uid"] = new JValue(uid),
            ["class_id"] = new JValue(classID),
            ["entity_type"] = new JValue(entityType),
        };
        if (rarityObject != null)
        {
            jsonData["rarity"] = rarityObject;
        }
        jsonData["container_position"] = new JValue(PosKey);
        jsonData["owner_uid"] = new JValue(ownerUid);
        if (mainPropsObject != null)
        {
            jsonData["main_props"] = mainPropsObject;
        }
        if (addonPropsObject != null)
        {
            jsonData["addon_props"] = addonPropsObject;
        }
        jsonData["is_world_equip"] = new JValue(0);
        jsonData["component_data"] = new JObject();
        return jsonData;
    }
}
