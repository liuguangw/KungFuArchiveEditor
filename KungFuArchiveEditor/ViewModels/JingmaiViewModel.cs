using KungFuArchiveEditor.GameConfig;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Collections.Generic;

namespace KungFuArchiveEditor.ViewModels;
public class JingmaiViewModel : ViewModelBase
{
    /// <summary>
    /// 经脉盘大小选项
    /// </summary>
    public class MapSizeOption
    {
        private readonly int sizeValue;
        public int Value => sizeValue;
        public string Name => sizeValue.ToString();

        public MapSizeOption(int value)
        {
            sizeValue = value;
        }
    }
    /// <summary>
    /// 经脉数据对象
    /// </summary>
    private JObject? jingmaiMapData = null;
    /// <summary>
    /// 大小
    /// </summary>
    private JValue? mapSizeObject = null;
    /// <summary>
    /// 页数
    /// </summary>
    private JValue? pageNumObject = null;
    /// <summary>
    /// 当前选择的页(从0开始)
    /// </summary>
    private JValue? activePageObject = null;
    private int slotCount = 0;
    private int placeCount = 6;
    private MapSizeOption selectedMapSize;

    public MapSizeOption SelectedMapSize
    {
        get => selectedMapSize;
        set
        {
            if (value == null)
            {
                return;
            }
            var newSize = value.Value;
            if (selectedMapSize.Value != newSize)
            {
                selectedMapSize = value;
                OnMapSizeChange();
                //重新设置json数据
                RebuildJsonData();
                NotifyReloadCanvas();
            }
        }
    }
    /// <summary>
    /// 穴位个数,不包含丹田
    /// </summary>
    public int SlotCount
    {
        get => slotCount;
        set
        {
            if (CheckRaiseAndSetIfChanged(ref slotCount, value))
            {
                this.RaisePropertyChanged(nameof(MaxLife));
            }
        }
    }
    /// <summary>
    /// 最大生命力
    /// </summary>
    public int MaxLife => placeCount - slotCount;

    public Dictionary<JingmaiNodePos, JingmaiNodeConfig> JingmaiMap { get; } = new();
    public List<MapSizeOption> MapSizeSelection { get; } = new();
    public JingmaiViewModel()
    {
        int i = 1;
        selectedMapSize = new MapSizeOption(i);
        MapSizeSelection.Add(selectedMapSize);
        do
        {
            i++;
            MapSizeSelection.Add(new MapSizeOption(i));
        } while (i < 11);
    }

    private void OnMapSizeChange()
    {
        this.RaisePropertyChanged(nameof(SelectedMapSize));
        placeCount = CalcPlaceCount(selectedMapSize.Value);
    }

    /// <summary>
    /// 根据经脉盘大小,计算位置总个数
    /// </summary>
    /// <param name="mapSize"></param>
    /// <returns></returns>
    private static int CalcPlaceCount(int mapSize)
    {
        int count = 0;
        for (int i = 1; i <= mapSize; i++)
        {
            count += i * 6;
        }
        return count;
    }

    /// <summary>
    /// 通知界面重绘
    /// </summary>
    private void NotifyReloadCanvas()
    {
        this.RaisePropertyChanged(nameof(JingmaiMap));
    }

    public void LoadJingmaiData(JObject jsonData)
    {
        var mapSizeField = jsonData["jingmai_map_size"];
        if (mapSizeField is JValue mapSizeValue)
        {
            mapSizeObject = mapSizeValue;
            var mapSize = mapSizeValue.ToObject<int>();
            foreach (var option in MapSizeSelection)
            {
                if (option.Value == mapSize)
                {
                    selectedMapSize = option;
                    OnMapSizeChange();
                    break;
                }
            }
        }
        var pageNumField = jsonData["jingmai_page_num"];
        if (pageNumField is JValue pageNumberValue)
        {
            pageNumObject = pageNumberValue;
        }
        var activePageField = jsonData["cur_active_jingmai_page"];
        if (activePageField is JValue activePageValue)
        {
            activePageObject = activePageValue;
        }
        //
        JingmaiMap.Clear();
        var mapData = jsonData["jingmai_map_data"];
        if (mapData is JObject mapDataObject)
        {
            jingmaiMapData = mapDataObject;
            LoadJingmaiMap(mapDataObject);
            NotifyReloadCanvas();
        }
    }

    /// <summary>
    /// 从json对象中解析经脉数据
    /// </summary>
    /// <param name="mapData"></param>
    private void LoadJingmaiMap(JObject mapData)
    {
        foreach (var keyValuePair in mapData)
        {
            var posStr = keyValuePair.Key;
            if (!JingmaiNodePos.TryParse(posStr, out var nodePos))
            {
                continue;
            }
            var detailObj = keyValuePair.Value;
            if (detailObj is JArray detailArr)
            {
                if (detailArr.Count < 2)
                {
                    continue;
                }
                var typeNode = detailArr[0];
                var nodeType = typeNode.ToObject<int>();
                var pageConfigNodes = detailArr[1];
                if (pageConfigNodes is JArray pageConfigArr)
                {
                    var pageLinkConfigs = ParsePageLinkConfigs(pageConfigArr);
                    JingmaiMap.Add(nodePos, new(nodeType, pageLinkConfigs));
                }
            }
        }
    }

    private static JingmaiNodeLinkInfo[] ParsePageLinkConfigs(JArray pageConfigArr)
    {
        var pageLinkConfigs = new JingmaiNodeLinkInfo[pageConfigArr.Count];
        for (int i = 0; i < pageConfigArr.Count; i++)
        {
            var pageConfigNode = pageConfigArr[i];
            if (pageConfigNode is JArray configNodes)
            {
                if (configNodes.Count < 3)
                {
                    continue;
                }
                var lineIndex = configNodes[0].ToObject<int>();
                if (lineIndex < 0)
                {
                    pageLinkConfigs[i] = JingmaiNodeLinkInfo.NullLink();
                    continue;
                }
                JingmaiNodePos? destPos = null;
                var destPosStr = configNodes[1].ToObject<string>();
                if (JingmaiNodePos.TryParse(destPosStr, out var tmpPos))
                {
                    destPos = tmpPos;
                }
                //
                JingmaiNodePos[]? srcPosList = null;
                var srcPosNodes = configNodes[2];
                if (srcPosNodes is JArray srcPosNodesArr)
                {
                    if (srcPosNodesArr.Count > 0)
                    {
                        srcPosList = new JingmaiNodePos[srcPosNodesArr.Count];
                        for (var j = 0; j < srcPosNodesArr.Count; j++)
                        {
                            var srcPosStr = srcPosNodesArr[j].ToObject<string>();
                            if (JingmaiNodePos.TryParse(srcPosStr, out var srcPos))
                            {
                                srcPosList[j] = srcPos;
                            }
                        }
                    }
                    pageLinkConfigs[i] = new(lineIndex, destPos, srcPosList);
                }

            }
        }
        return pageLinkConfigs;
    }

    public void RebuildJsonData()
    {
        int mapSize = selectedMapSize.Value;
        //判断连线是否超出经脉盘,防住经脉盘缩小时出现问题
        bool isOutOfMap = false;
        foreach (var nodeConfig in JingmaiMap.Values)
        {
            if (nodeConfig.IsOutOfMap(mapSize))
            {
                isOutOfMap = true;
                break;
            }
        }
        var zeroPos = JingmaiNodePos.Zero();
        var zeroPosConfig = JingmaiMap[zeroPos];
        int pageNum = zeroPosConfig.PageLinkConfigs.Length;
        //连线超出范围则取消所有页的连线
        if (isOutOfMap)
        {
            pageNum = 1;
        }

        if (mapSizeObject != null)
        {
            mapSizeObject.Value = mapSize;
        }
        if (pageNumObject != null)
        {
            pageNumObject.Value = pageNum;
        }
        if (isOutOfMap && activePageObject != null)
        {
            activePageObject.Value = 0;
        }
        if (jingmaiMapData == null)
        {
            return;
        }
        //清理json对象中的所有key
        //不能遍历集合的时候修改集合
        //先取出key再删除
        var keyList = new List<string>();
        foreach (var pair in jingmaiMapData)
        {
            keyList.Add(pair.Key);
        }
        foreach (var key in keyList)
        {
            jingmaiMapData.Remove(key);
        }
        int rBegin = 0;
        for (int q = -mapSize; q <= 0; q++)
        {
            for (int r = rBegin; r <= mapSize; r++)
            {
                SetJingmaiMapData(jingmaiMapData, q, r, isOutOfMap);
                //Debug.Write($"q={q},r={r} ");
            }
            //Debug.WriteLine("\n=====");
            rBegin--;
        }
        rBegin = -mapSize;
        int rEnd = mapSize - 1;
        for (int q = 1; q <= mapSize; q++)
        {
            for (int r = rBegin; r <= rEnd; r++)
            {
                SetJingmaiMapData(jingmaiMapData, q, r, isOutOfMap);
                //Debug.Write($"q={q},r={r} ");
            }
            //Debug.WriteLine("\n=====");
            rEnd--;
        }
    }

    private void SetJingmaiMapData(JObject mapObject, int q, int r, bool isOutOfMap)
    {
        var posKey = new JingmaiNodePos(q, r);
        if (!JingmaiMap.TryGetValue(posKey, out var mapNodeConfig))
        {
            return;
        }
        var nodeConfig = mapNodeConfig;
        if (isOutOfMap)
        {
            //去掉连线数据
            var linkConfigs = new JingmaiNodeLinkInfo[]
            {
                JingmaiNodeLinkInfo.NullLink()
            };
            nodeConfig = new(mapNodeConfig.NodeType, linkConfigs);
        }
        var mapKeyStr = posKey.ToString();
        var mapValue = nodeConfig.ToJsonArray();
        mapObject[mapKeyStr] = mapValue;
    }
}
