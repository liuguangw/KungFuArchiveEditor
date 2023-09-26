namespace KungFuArchiveEditor.GameConfig;

/// <summary>
/// 经脉点的位置
/// </summary>
public readonly struct JingmaiNodePos
{
    private static readonly JingmaiNodePos zeroPos = new(0, 0);
    private readonly int posQ;
    private readonly int posR;

    public readonly int PosQ => posQ;
    public readonly int PosR => posR;
    public readonly int PosS => -posQ - posR;

    public JingmaiNodePos(int posQ, int posR)
    {
        this.posQ = posQ;
        this.posR = posR;
    }

    /// <summary>
    /// 判断节点是否超出了经脉盘的范围,防止缩小的时候出现问题
    /// </summary>
    /// <param name="mapSize"></param>
    /// <returns></returns>
    public bool IsOutOfMap(int mapSize)
    {
        if (posQ < -mapSize || posQ > mapSize)
        {
            return true;
        }
        if (posR < -mapSize || posR > mapSize)
        {
            return true;
        }
        if (PosS < -mapSize || PosS > mapSize)
        {
            return true;
        }
        return false;
    }

    public bool IsZero()
    {
        return posQ == 0 && posR == 0;
    }

    public override string ToString()
    {
        return $"{posQ}|{posR}|{PosS}";
    }

    public static JingmaiNodePos Zero() => zeroPos;

    public static bool TryParse(string? posStr, out JingmaiNodePos posInfo)
    {
        if (string.IsNullOrEmpty(posStr))
        {
            posInfo = zeroPos;
            return false;
        }
        var posValues = posStr.Split('|');
        if (posValues.Length < 3)
        {
            posInfo = zeroPos;
            return false;
        }
        if (!int.TryParse(posValues[0], out var posQ))
        {
            posInfo = zeroPos;
            return false;
        }
        if (!int.TryParse(posValues[1], out var posR))
        {
            posInfo = zeroPos;
            return false;
        }
        posInfo = new(posQ, posR);
        return true;
    }
}
