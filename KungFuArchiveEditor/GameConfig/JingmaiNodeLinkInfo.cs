using Newtonsoft.Json.Linq;

namespace KungFuArchiveEditor.GameConfig;
/// <summary>
/// 经脉点的连接信息
/// </summary>
public class JingmaiNodeLinkInfo
{
    private static readonly JingmaiNodeLinkInfo nullLink = new(-1, null, null);
    public readonly int LineIndex;
    /// <summary>
    /// 下一个点的位置
    /// </summary>
    public readonly JingmaiNodePos? DestPos;
    /// <summary>
    /// 上一个点的位置
    /// </summary>
    public readonly JingmaiNodePos[]? SrcPosList;

    public JingmaiNodeLinkInfo(int lineIndex, JingmaiNodePos? destPos, JingmaiNodePos[]? srcPosList)
    {
        LineIndex = lineIndex;
        DestPos = destPos;
        SrcPosList = srcPosList;
    }

    /// <summary>
    /// 转换为json数组的格式
    /// </summary>
    /// <returns></returns>
    public JArray ToJsonArray()
    {
        var arr = new JArray() {
            new JValue(LineIndex)
        };
        //
        var destPosStr = string.Empty;
        if (DestPos != null)
        {
            destPosStr = DestPos.Value.ToString();
        }
        arr.Add(new JValue(destPosStr));
        //
        var srcPosListArr = new JArray();
        if (SrcPosList != null)
        {
            foreach (var pos in SrcPosList)
            {
                srcPosListArr.Add(new JValue(pos.ToString()));
            }
        }
        arr.Add(srcPosListArr);
        return arr;
    }
    /// <summary>
    /// 孤立节点
    /// </summary>
    /// <returns></returns>
    public static JingmaiNodeLinkInfo NullLink() => nullLink;
}
