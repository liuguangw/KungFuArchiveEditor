using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.Tools;

public static class GameMetaData
{
    private static Dictionary<int, string> itemNames = new();
    public static async Task LoadAsync()
    {
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDirPath = Path.GetDirectoryName(exePath);
        if (string.IsNullOrEmpty(exeDirPath))
        {
            return;
        }
        var itemNamesFilePath = Path.Combine(exeDirPath, "item_names.txt");
        if (!File.Exists(itemNamesFilePath))
        {
            throw new Exception("未找到物品名称配置文件: " + itemNamesFilePath);
        }
        try
        {

            await LoadItemNamesAsync(itemNamesFilePath);
        }
        catch (Exception ex)
        {
            throw new Exception("解析物品名称配置文件出错, " + ex.Message);
        }
    }

    private static async Task LoadItemNamesAsync(string filePath)
    {
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
                CheckAddItem(tmpTexts[0], tmpTexts[1]);
            }
        }
    }

    private static void CheckAddItem(string key, string value)
    {
        var classID = int.Parse(key);
        if (itemNames.ContainsKey(classID))
        {
            throw new Exception($"ID: {classID}重复");
        }
        itemNames.Add(classID, value);
    }

    public static string? GetItemName(int classID)
    {
        if (itemNames.ContainsKey(classID))
        {
            return itemNames[classID];
        }
        return null;
    }
}
