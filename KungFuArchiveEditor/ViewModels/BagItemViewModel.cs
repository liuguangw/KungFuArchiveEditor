using KungFuArchiveEditor.GameConfig;
using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;
using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KungFuArchiveEditor.Assets;

namespace KungFuArchiveEditor.ViewModels;

public class BagItemViewModel : ViewModelBase, INotifyDataErrorInfo
{
    #region Fields
    protected int mainPos = 0;
    protected int subPos = 0;
    protected int pos = 0;
    protected int classID = 0;
    protected int entityType = 0;
    protected int amount = 1;

    protected JValue? amountObject = null;
    private readonly List<ValidationResult> errors = new();
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Properties
    public bool HasErrors => errors.Count > 0;
    public int MainPos => mainPos;
    public int SubPos => subPos;
    public int Pos => pos;

    public string PosKey => $"{mainPos}-{subPos}-{pos}";

    public int ClassID => classID;

    public string Name => GetItemName(classID);
    public int EntityType => entityType;
    public int Amount
    {
        get => amount;
        set
        {
            if (HasErrors)
            {
                errors.Clear();
                RaiseErrorsChanged();
            }
            var maxAmount = GetMaxAmount(classID);
            if (value < 0 || value > maxAmount)
            {
                errors.Add(new ValidationResult(LangResources.Amount + " <= " + maxAmount.ToString()));
                RaiseErrorsChanged();
            }
            RaiseAndSetIfChanged(ref amount, value, amountObject);
        }
    }

    /// <summary>
    /// 是否可以修改amount
    /// </summary>
    public bool HasAmount => GetMaxAmount(classID) > 1;
    #endregion

    /// <summary>
    /// 从json对象中读取装备属性
    /// </summary>
    /// <param name="posArr">位置(3个数字)</param>
    /// <param name="itemEntityType">实体类型</param>
    /// <param name="jsonData">json对象</param>
    public virtual void LoadItemData(int[] posArr, int itemEntityType, JToken jsonData)
    {
        mainPos = posArr[0];
        subPos = posArr[1];
        pos = posArr[2];
        entityType = itemEntityType;
        //class_id
        var objectNode = jsonData["class_id"];
        if (objectNode != null)
        {
            classID = objectNode.ToObject<int>();
        }
        //amount
        objectNode = jsonData["amount"];
        if (objectNode != null)
        {
            amount = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                amountObject = value;
            }
        }
    }

    /// <summary>
    /// 初始化新的物品对象
    /// </summary>
    /// <param name="posArr"></param>
    /// <param name="classID"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public virtual void InitItem(int[] posArr, int classID, int amount = 1)
    {
        mainPos = posArr[0];
        subPos = posArr[1];
        pos = posArr[2];
        entityType = 7;
        this.classID = classID;
        this.amount = amount;
        amountObject = new JValue(amount);
    }

    /// <summary>
    /// 转换为新的json对象
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="ownerUid"></param>
    /// <returns></returns>
    public virtual JObject ToNewJsonData(string uid, string ownerUid)
    {
        var jsonData = new JObject
        {
            ["uid"] = new JValue(uid),
            ["class_id"] = new JValue(classID),
            ["entity_type"] = new JValue(entityType),
            ["container_position"] = new JValue(PosKey)
        };
        if (amountObject != null)
        {
            jsonData["amount"] = amountObject;
        }
        jsonData["owner_uid"] = new JValue(ownerUid);
        var emptyFields = new string[]
        {
            "origin_uid","origin_owner_name","murderer_name","fabricator_uid","murderer_uid",
            "key_owner_uid","horse_uid"
        };
        foreach (var fieldName in emptyFields)
        {
            jsonData.Add(fieldName, new JValue(string.Empty));
        }
        jsonData["tile_id"] = new JValue(0);
        jsonData["is_world_item"] = new JValue(0);
        jsonData["component_data"] = new JObject();
        return jsonData;
    }

    public virtual string GetItemName(int classID)
    {
        GameConfigData.Items.TryGetValue(classID, out ItemConfig? itemConfig);
        if (itemConfig != null)
        {
            return itemConfig.Name;
        }
        return "未知";
    }

    private static int GetMaxAmount(int classID)
    {
        GameConfigData.Items.TryGetValue(classID, out ItemConfig? itemConfig);
        if (itemConfig != null)
        {
            return itemConfig.MaxAmount;
        }
        return 1;
    }
    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName == nameof(Amount))
        {
            return errors;
        }
        return Array.Empty<ValidationResult>();
    }

    /// <summary>
    /// 通知错误状态更新
    /// </summary>
    private void RaiseErrorsChanged()
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Amount)));
    }
}
