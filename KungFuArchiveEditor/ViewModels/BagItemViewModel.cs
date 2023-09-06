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
    public bool HasAmount => hasAmount;
    #endregion

    /// <summary>
    /// 从json对象中加载物品信息
    /// </summary>
    /// <param name="jsonData"></param>
    public virtual void LoadItemData(int[] posArr,int itemEntityType, JToken jsonData)
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
        return GameMetaData.GetItemName(classID) ?? $"未知(class_id:{classID})";
    }
}
