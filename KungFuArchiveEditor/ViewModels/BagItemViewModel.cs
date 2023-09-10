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
    private int mainPos = 0;
    private int subPos = 0;
    private int pos = 0;
    private int classID = 0;
    private int entityType = 0;
    private int amount = 1;

    protected JValue? amountObject = null;
    protected JToken? itemJsonData = null;
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
        itemJsonData = jsonData;
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
