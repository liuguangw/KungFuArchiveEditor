using KungFuArchiveEditor.GameConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.Tools;

public static class GameConfigData
{
    public static readonly Dictionary<int, ItemConfig> Items = new();
    public static readonly Dictionary<int, EquipConfig> Equips = new();
    public static readonly Dictionary<int, MainPropConfig> EquipMainProps = new();
    public static readonly Dictionary<int, string> EquipAddonProps = new();
    public static readonly Dictionary<int, AbilityConfig> Abilities = new();
    public static async Task LoadAsync()
    {
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDirPath = Path.GetDirectoryName(exePath);
        if (string.IsNullOrEmpty(exeDirPath))
        {
            throw new Exception("未找到安装文件夹路径");
        }
        var dataPath = Path.Combine(exeDirPath, "Data");
        var dataColl = new (string, int, Action<string[]>)[]
        {
            ("item.txt",3,ItemRowCallback),
            ("equipment.txt",3,EquipRowCallback),
            ("equip_main_prop.txt",3,MainPropRowCallback),
            ("equip_addon_prop.txt",2,fields => CommonRowCallback(fields,EquipAddonProps)),
            ("ability.txt",3,AbilityRowCallback)
        };
        foreach (var item in dataColl)
        {
            var (fileName, maxCount, rowCallback) = item;
            var configFilePath = Path.Combine(dataPath, fileName);
            try
            {
                await LoadLineNodesAsync(configFilePath, maxCount, rowCallback);
            }
            catch (Exception ex)
            {
                throw new Exception("解析名称配置文件出错, " + ex.Message + ", path=" + configFilePath);
            }
        }
    }
    /// <summary>
    /// 道具
    /// </summary>
    /// <param name="fields"></param>
    private static void ItemRowCallback(string[] fields)
    {
        if (fields.Length < 3)
        {
            return;
        }
        var classIDStr = fields[0].Trim();
        var maxAmountStr = fields[1].Trim();
        var name = fields[2].Trim();
        //
        if (string.IsNullOrEmpty(classIDStr) || string.IsNullOrEmpty(maxAmountStr) || string.IsNullOrEmpty(name))
        {
            return;
        }
        var classID = int.Parse(classIDStr);
        var maxAmount = int.Parse(maxAmountStr);
        Items.Add(classID, new ItemConfig(classID, maxAmount, name));
    }
    /// <summary>
    /// 装备
    /// </summary>
    /// <param name="fields"></param>
    private static void EquipRowCallback(string[] fields)
    {
        if (fields.Length < 3)
        {
            return;
        }
        var classIDStr = fields[0].Trim();
        var equipTypeStr = fields[1].Trim();
        var name = fields[2].Trim();
        //
        if (string.IsNullOrEmpty(classIDStr) || string.IsNullOrEmpty(equipTypeStr) || string.IsNullOrEmpty(name))
        {
            return;
        }
        var classID = int.Parse(classIDStr);
        var equipType = int.Parse(equipTypeStr);
        Equips.Add(classID, new EquipConfig(classID, equipType, name));
    }
    /// <summary>
    /// 主属性
    /// </summary>
    /// <param name="fields"></param>
    private static void MainPropRowCallback(string[] fields)
    {
        if (fields.Length < 3)
        {
            return;
        }
        var classIDStr = fields[0].Trim();
        var equipTypeStr = fields[1].Trim();
        var name = fields[2].Trim();
        //
        if (string.IsNullOrEmpty(classIDStr) || string.IsNullOrEmpty(equipTypeStr) || string.IsNullOrEmpty(name))
        {
            return;
        }
        var classID = int.Parse(classIDStr);
        var equipType = int.Parse(equipTypeStr);
        EquipMainProps.Add(classID, new MainPropConfig(classID, equipType, name));
    }
    /// <summary>
    /// 能力
    /// </summary>
    /// <param name="fields"></param>
    private static void AbilityRowCallback(string[] fields)
    {
        if (fields.Length < 3)
        {
            return;
        }
        var classIDStr = fields[0].Trim();
        var maxLevelStr = fields[1].Trim();
        var name = fields[2].Trim();
        //
        if (string.IsNullOrEmpty(classIDStr) || string.IsNullOrEmpty(maxLevelStr) || string.IsNullOrEmpty(name))
        {
            return;
        }
        var classID = int.Parse(classIDStr);
        var maxLevel = int.Parse(maxLevelStr);
        Abilities.Add(classID, new AbilityConfig(classID, maxLevel, name));
    }
    /// <summary>
    /// 只有id和名称两列的回调函数
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="targetMap"></param>
    private static void CommonRowCallback(string[] fields, Dictionary<int, string> targetMap)
    {
        if (fields.Length < 2)
        {
            return;
        }
        var classIDStr = fields[0].Trim();
        var name = fields[1].Trim();
        if (string.IsNullOrEmpty(classIDStr) || string.IsNullOrEmpty(name))
        {
            return;
        }
        var classID = int.Parse(classIDStr);
        targetMap.Add(classID, name);
    }
    /// <summary>
    /// 加载文本文件,并在非注释行调用rowCallback
    /// </summary>
    /// <param name="filePath">文本文件路径</param>
    /// <param name="maxCount">最大几列</param>
    /// <param name="rowCallback">回调函数</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task LoadLineNodesAsync(string filePath, int maxCount, Action<string[]> rowCallback)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception("未找到配置文件: " + filePath);
        }
        using var streamReader = new StreamReader(filePath, Encoding.UTF8);
        var lineSeps = new[] { ' ', '\t' };
        string? lineContent;
        string[] fields;
        while (true)
        {
            lineContent = await streamReader.ReadLineAsync();
            if (lineContent == null)
            {
                break;
            }
            lineContent = lineContent.Trim();
            //忽略空行
            if (string.IsNullOrEmpty(lineContent))
            {
                continue;
            }
            //注释行
            if (lineContent.StartsWith('#'))
            {
                continue;
            }
            fields = lineContent.Split(lineSeps, maxCount);
            rowCallback.Invoke(fields);
        }
    }
}
