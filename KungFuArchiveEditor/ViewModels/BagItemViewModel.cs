using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;

namespace KungFuArchiveEditor.ViewModels;

public class BagItemViewModel : ViewModelBase
{
    #region Fields
    private string posKey = string.Empty;
    private int classID = 0;
    private int entityType = 0;
    private int amount = 1;
    private bool hasAmount = false;

    protected JValue? classIDObject = null;
    protected JValue? entityTypeObject = null;
    protected JValue? amountObject = null;
    #endregion

    #region Properties
    public string PosKey
    {
        get => posKey;
        set => CheckRaiseAndSetIfChanged(ref posKey, value);
    }
    public int ClassID
    {
        get => classID;
        set => RaiseAndSetIfChanged(ref classID, value, classIDObject);
    }

    public string Name => GameMetaData.GetItemName(classID) ?? $"未知(class_id:{classID})";
    public int EntityType
    {
        get => entityType;
        set => RaiseAndSetIfChanged(ref entityType, value, entityTypeObject);
    }
    public int Amount
    {
        get => amount;
        set => RaiseAndSetIfChanged(ref amount, value, amountObject);
    }
    public bool HasAmount
    {
        get => hasAmount;
        set => CheckRaiseAndSetIfChanged(ref hasAmount, value);
    }
    #endregion

    /// <summary>
    /// 从json对象中加载物品信息
    /// </summary>
    /// <param name="jsonData"></param>
    public virtual void LoadItemData(JToken jsonData)
    {
        //class_id
        var objectNode = jsonData["class_id"];
        if (objectNode != null)
        {
            ClassID = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                classIDObject = value;
            }
        }
        //entity_type
        objectNode = jsonData["entity_type"];
        if (objectNode != null)
        {
            EntityType = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                entityTypeObject = value;
            }
        }
        //amount
        objectNode = jsonData["amount"];
        if (objectNode != null)
        {
            Amount = objectNode.ToObject<int>();
            HasAmount = true;
            if (objectNode is JValue value)
            {
                amountObject = value;
            }
        }
        else
        {
            Amount = 1;
            HasAmount = false;
        }

    }
}
