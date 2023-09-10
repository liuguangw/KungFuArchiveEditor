namespace KungFuArchiveEditor.GameConfig;
public class EquipConfig
{
    public readonly int ID;
    /// <summary>
    /// 装备类型
    /// </summary>
    public readonly int EquipType;
    /// <summary>
    /// 名称
    /// </summary>
    public readonly string Name;

    public EquipConfig(int id, int equipType, string name)
    {
        ID = id;
        EquipType = equipType;
        Name = name;
    }
}
