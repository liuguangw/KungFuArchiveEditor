using Newtonsoft.Json.Linq;

namespace KungFuArchiveEditor.ViewModels;

public class RoleViewModel : ViewModelBase
{
    #region Fields
    private string userID = string.Empty;
    private string name = string.Empty;
    private int maxHP = 0;
    private int maxZhenqi = 0;
    private int maxBurden = 0;

    private  JValue? nameObject = null;
    private  JValue? maxHPObject= null;
    private  JValue? maxZhenqiObject= null;
    private  JValue? maxBurdenObject= null;
    #endregion

    #region Properties
    public string UserID
    {
        get => userID;
        set => CheckRaiseAndSetIfChanged(ref userID, value);
    }

    public string Name
    {
        get => name;
        set => RaiseAndSetIfChanged(ref name, value, nameObject);
    }
    public int MaxHP
    {
        get => maxHP;
        set => RaiseAndSetIfChanged(ref maxHP, value, maxHPObject);
    }
    public int MaxZhenqi
    {
        get => maxZhenqi;
        set => RaiseAndSetIfChanged(ref maxZhenqi, value, maxZhenqiObject);
    }
    public int MaxBurden
    {
        get => maxBurden;
        set => RaiseAndSetIfChanged(ref maxBurden, value, maxBurdenObject);
    }
    #endregion

    /// <summary>
    /// 从json对象中加载数据
    /// </summary>
    /// <param name="playerData"></param>
    public void LoadPlayerData(JToken playerData)
    {
        //name
        var objectNode = playerData["name"];
        if (objectNode != null)
        {
            Name = objectNode.ToObject<string>()!;
            if(objectNode is JValue value)
            {
                nameObject = value;
            }
        }
        //max hp
        objectNode = playerData["max_hp"];
        if (objectNode != null)
        {
            MaxHP = objectNode.ToObject<int>()!;
            if (objectNode is JValue value)
            {
                maxHPObject = value;
            }
        }
        //max zhenqi
        objectNode = playerData["max_zhenqi"];
        if (objectNode != null)
        {
            MaxZhenqi = objectNode.ToObject<int>()!;
            if (objectNode is JValue value)
            {
                maxZhenqiObject = value;
            }
        }
        //max burden
        objectNode = playerData["max_burden"];
        if (objectNode != null)
        {
            MaxBurden = objectNode.ToObject<int>()!;
            if (objectNode is JValue value)
            {
                maxBurdenObject = value;
            }
        }
    }
}
