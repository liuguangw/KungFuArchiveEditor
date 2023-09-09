namespace KungFuArchiveEditor.GameConfig;

/// <summary>
/// 能力配置
/// </summary>
public class AbilityConfig
{
    public readonly int ID;
    /// <summary>
    /// 最大等级
    /// </summary>
    public readonly int MaxLevel;
    /// <summary>
    /// 名称
    /// </summary>
    public readonly string Name;

    public AbilityConfig(int id, int maxLevel, string name)
    {
        ID = id;
        MaxLevel = maxLevel;
        Name = name;
    }
}
