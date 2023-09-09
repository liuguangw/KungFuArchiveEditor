using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;

namespace KungFuArchiveEditor.ViewModels;

public class BagItemViewModel : ViewModelBase
{
    #region Fields
    private int mainPos = 0;
    private int subPos = 0;
    private int pos = 0;
    private int classID = 0;
    private int entityType = 0;
    private int amount = 1;
    private bool hasAmount = false;

    protected JValue? amountObject = null;
    protected JToken? itemJsonData = null;
    #endregion

    #region Properties
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
        set => RaiseAndSetIfChanged(ref amount, value, amountObject);
    }

    /// <summary>
    /// 是否有amount这个字段
    /// </summary>
    public bool HasAmount => hasAmount;
    #endregion

    /// <summary>
    /// 从json对象中读取装备属性
    /// </summary>
    /// <param name="posArr">位置(3个数字)</param>
    /// <param name="itemEntityType">实体类型</param>
    /// <param name="jsonData">json对象</param>
    public virtual void LoadItemData(int[] posArr,int itemEntityType, JToken jsonData)
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
            hasAmount = true;
            if (objectNode is JValue value)
            {
                amountObject = value;
            }
        }
        else
        {
            amount = 1;
            hasAmount = false;
        }

    }

    public virtual string GetItemName(int classID)
    {
        GameConfigData.Items.TryGetValue(classID, out string? itemName);
        if (itemName != null)
        {
            return itemName;
        }
        return "未知";
    }
}
