using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.Tools;

public static class GameMetaData
{
    public enum MetaType
    {
        Item,
        Equip,
        EquipAddonProp
    }
    private static readonly Dictionary<int, string> itemNames = new();
    private static readonly Dictionary<int, string> equipNames = new();
    private static readonly Dictionary<int, string> equipPropNames = new();
    public static async Task LoadAsync()
    {
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDirPath = Path.GetDirectoryName(exePath);
        if (string.IsNullOrEmpty(exeDirPath))
        {
            return;
        }
        var dataPath = Path.Combine(exeDirPath, "Data");
        var fileMap = new Dictionary<string, Dictionary<int, string>>
        {
            ["item.txt"] = itemNames,
            ["equipment.txt"] = equipNames,
            ["equip_addon_prop.txt"] = equipPropNames
        };
        foreach (var keyPair in fileMap)
        {
            var configFilePath = Path.Combine(dataPath, keyPair.Key);
            try
            {
                await LoadItemNamesAsync(configFilePath, keyPair.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("解析名称配置文件出错, " + ex.Message + ", path=" + configFilePath);
            }
        }
    }

    private static async Task LoadItemNamesAsync(string filePath, Dictionary<int, string> targetMap)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception("未找到名称配置文件: " + filePath);
        }
        using var streamReader = new StreamReader(filePath, Encoding.UTF8);
        //id与名称的分隔符
        var lineSeps = new[] { ' ', '\t' };
        string? lineContent;
        string[] tmpTexts;
        while (true)
        {
            lineContent = await streamReader.ReadLineAsync();
            if (lineContent == null)
            {
                break;
            }
            lineContent = lineContent.Trim();
            //注释行
            if (lineContent.StartsWith('#'))
            {
                continue;
            }
            tmpTexts = lineContent.Split(lineSeps, 2);
            if (tmpTexts.Length > 1)
            {
                tmpTexts[1] = tmpTexts[1].Trim();
                if (string.IsNullOrEmpty(tmpTexts[0]) || string.IsNullOrEmpty(tmpTexts[1]))
                {
                    continue;
                }
                CheckAddItem(tmpTexts[0], tmpTexts[1], targetMap);
            }
        }
    }

    private static void CheckAddItem(string key, string value, Dictionary<int, string> targetMap)
    {
        var classID = int.Parse(key);
        if (targetMap.ContainsKey(classID))
        {
            throw new Exception($"ID: {classID}重复");
        }
        targetMap.Add(classID, value);
    }

    public static string? GetItemName(int classID, MetaType itemMetaType = MetaType.Item)
    {
        var targetMap = itemNames;
        if (itemMetaType == MetaType.Equip)
        {
            targetMap = equipNames;
        }
        else if (itemMetaType == MetaType.EquipAddonProp)
        {
            targetMap = equipPropNames;
        }

        if (targetMap.ContainsKey(classID))
        {
            return targetMap[classID];
        }
        return null;
    }
}
