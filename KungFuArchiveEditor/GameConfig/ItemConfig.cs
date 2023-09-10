namespace KungFuArchiveEditor.GameConfig;
public class ItemConfig
{
    public readonly int ID;
    /// <summary>
    /// 最大叠加数量
    /// </summary>
    public readonly int MaxAmount;
    /// <summary>
    /// 名称
    /// </summary>
    public readonly string Name;

    public ItemConfig(int id, int maxAmount, string name)
    {
        ID = id;
        MaxAmount = maxAmount;
        Name = name;
    }
}
