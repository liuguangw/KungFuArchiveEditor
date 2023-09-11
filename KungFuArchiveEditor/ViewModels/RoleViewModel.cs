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
    private int zhangfaAttack = 0;
    private int quanfaAttack = 0;
    private int tuifaAttack = 0;
    private int bingqiAttack = 0;
    private int anqiAttack = 0;

    private JValue? nameObject = null;
    private JValue? maxHPObject = null;
    private JValue? maxZhenqiObject = null;
    private JValue? maxBurdenObject = null;
    private JValue? zhangfaAttackObject = null;
    private JValue? quanfaAttackObject = null;
    private JValue? tuifaAttackObject = null;
    private JValue? bingqiAttackObject = null;
    private JValue? anqiAttackObject = null;
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
    public int ZhangfaAttack { get => zhangfaAttack; set => RaiseAndSetIfChanged(ref zhangfaAttack, value, zhangfaAttackObject); }
    public int QuanfaAttack { get => quanfaAttack; set => RaiseAndSetIfChanged(ref quanfaAttack, value, quanfaAttackObject); }
    public int TuifaAttack { get => tuifaAttack; set => RaiseAndSetIfChanged(ref tuifaAttack, value, tuifaAttackObject); }
    public int BingqiAttack { get => bingqiAttack; set => RaiseAndSetIfChanged(ref bingqiAttack, value, bingqiAttackObject); }
    public int AnqiAttack { get => anqiAttack; set => RaiseAndSetIfChanged(ref anqiAttack, value, anqiAttackObject); }
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
            if (objectNode is JValue value)
            {
                nameObject = value;
            }
        }
        //max hp
        objectNode = playerData["max_hp"];
        if (objectNode != null)
        {
            MaxHP = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                maxHPObject = value;
            }
        }
        //max zhenqi
        objectNode = playerData["max_zhenqi"];
        if (objectNode != null)
        {
            MaxZhenqi = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                maxZhenqiObject = value;
            }
        }
        //max burden
        objectNode = playerData["max_burden"];
        if (objectNode != null)
        {
            MaxBurden = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                maxBurdenObject = value;
            }
        }
        //zhangfa_attack
        objectNode = playerData["zhangfa_attack"];
        if (objectNode != null)
        {
            ZhangfaAttack = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                zhangfaAttackObject = value;
            }
        }
        //quanfa_attack
        objectNode = playerData["quanfa_attack"];
        if (objectNode != null)
        {
            QuanfaAttack = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                quanfaAttackObject = value;
            }
        }
        //tuifa_attack
        objectNode = playerData["tuifa_attack"];
        if (objectNode != null)
        {
            TuifaAttack = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                tuifaAttackObject = value;
            }
        }
        //bingqi_attack
        objectNode = playerData["bingqi_attack"];
        if (objectNode != null)
        {
            BingqiAttack = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                bingqiAttackObject = value;
            }
        }
        //anqi_attack
        objectNode = playerData["anqi_attack"];
        if (objectNode != null)
        {
            AnqiAttack = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                anqiAttackObject = value;
            }
        }
    }
}
