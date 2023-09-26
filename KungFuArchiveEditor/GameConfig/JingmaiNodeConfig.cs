using Newtonsoft.Json.Linq;

namespace KungFuArchiveEditor.GameConfig;
public class JingmaiNodeConfig
{
    public int NodeType;
    public JingmaiNodeLinkInfo[] PageLinkConfigs;

    public JingmaiNodeConfig(int nodeType, JingmaiNodeLinkInfo[] pageLinkConfigs)
    {
        NodeType = nodeType;
        PageLinkConfigs = pageLinkConfigs;
    }
    public bool IsOutOfMap(int mapSize)
    {
        foreach (var pageConfig in PageLinkConfigs)
        {
            if (pageConfig.LineIndex < 0)
            {
                continue;
            }
            //dest pos
            if (pageConfig.DestPos.HasValue)
            {
                if (pageConfig.DestPos.Value.IsOutOfMap(mapSize))
                {
                    return true;
                }
            }
            //src list
            if (pageConfig.SrcPosList != null)
            {
                foreach (var pos in pageConfig.SrcPosList)
                {
                    if (pos.IsOutOfMap(mapSize)) { return true; }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 转换为json数组的格式
    /// </summary>
    /// <returns></returns>
    public JArray ToJsonArray()
    {
        var arr = new JArray() {
            new JValue(NodeType)
        };
        var pageConfigsArr = new JArray();
        foreach (var pageConfig in PageLinkConfigs)
        {
            pageConfigsArr.Add(pageConfig.ToJsonArray());
        }
        arr.Add(pageConfigsArr);
        return arr;
    }
}
