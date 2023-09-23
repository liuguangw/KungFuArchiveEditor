using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Collections.Generic;

namespace KungFuArchiveEditor.ViewModels;
public class JingmaiViewModel : ViewModelBase
{
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
    private int mapSize = 1;

    public int MapSize
    {
        get => mapSize;
        set
        {
            if (value < 1)
            {
                return;
            }
            if (value != mapSize)
            {
                int oldValue = mapSize;
                mapSize = value;
                ProcessMapSizeChange(oldValue, value);
                NotifyReloadCanvas();
            }
        }
    }

    public Dictionary<(int, int, int), int> JingmaiMap { get; } = new();

    /// <summary>
    /// 通知界面重绘
    /// </summary>
    private void NotifyReloadCanvas()
    {
        this.RaisePropertyChanged(nameof(JingmaiMap));
    }

    private void ProcessMapSizeChange(int oldValue, int newValue)
    {
        if (mapSizeObject != null)
        {
            mapSizeObject.Value = newValue;
        }
        //扩容模式
        if (newValue > oldValue)
        {
            return;
        }
        //缩减模式, 重新设置json数据
        RebuildJsonData();
    }

    public void LoadJingmaiData(JObject jsonData)
    {
        var mapSizeField = jsonData["jingmai_map_size"];
        if (mapSizeField is JValue mapSizeValue)
        {
            mapSizeObject = mapSizeValue;
            mapSize = mapSizeValue.ToObject<int>();
            this.RaisePropertyChanged(nameof(MapSize));
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
            var posValues = keyValuePair.Key.Split('|');
            if (posValues.Length < 3)
            {
                continue;
            }
            var pos = (int.Parse(posValues[0]), int.Parse(posValues[1]), int.Parse(posValues[2]));
            var detailObj = keyValuePair.Value;
            if (detailObj is JArray detailArr)
            {
                if (detailArr.Count < 1)
                {
                    continue;
                }
                var typeNode = detailArr[0];
                var typeValue = typeNode.ToObject<int>();
                JingmaiMap.Add(pos, typeValue);
            }
        }
    }

    public void RebuildJsonData()
    {
        if (mapSizeObject != null)
        {
            mapSizeObject.Value = mapSize;
        }
        if (pageNumObject != null)
        {
            pageNumObject.Value = 1;
        }
        if (activePageObject != null)
        {
            activePageObject.Value = 0;
        }
        if (jingmaiMapData == null)
        {
            return;
        }
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
                SetJingmaiMapData(jingmaiMapData, q, r);
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
                SetJingmaiMapData(jingmaiMapData, q, r);
                //Debug.Write($"q={q},r={r} ");
            }
            //Debug.WriteLine("\n=====");
            rEnd--;
        }
    }

    private void SetJingmaiMapData(JObject mapObject, int q, int r)
    {
        int s = -q - r;
        var mapKey = (q, r, s);
        if (mapKey == (0, 0, 0))
        {
            //丹田
            SetJingmaiType(mapObject, mapKey, 1);
            return;

        }
        if (!JingmaiMap.TryGetValue(mapKey, out int nodeType))
        {
            return;
        }
        //3种穴位
        if (nodeType == 2 || nodeType == 3 || nodeType == 4)
        {
            SetJingmaiType(mapObject, mapKey, nodeType);
        }
    }

    private static void SetJingmaiType(JObject mapObject, (int, int, int) mapKey, int nodeType)
    {
        var (q, r, s) = mapKey;
        var mapKeyStr = $"{q}|{r}|{s}";
        var mapValue = new JArray() {
            new JValue(nodeType),
            new JArray() {
                new JArray() {
                    new JValue(-1),
                    new JValue(string.Empty),
                    new JArray() {}
                }
            }
        };
        mapObject[mapKeyStr] = mapValue;
    }
}
