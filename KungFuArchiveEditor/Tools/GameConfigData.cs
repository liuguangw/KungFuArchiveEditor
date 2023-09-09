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
    public static readonly Dictionary<int, string> Items = new();
    public static readonly Dictionary<int, string> Equips = new();
    public static readonly Dictionary<int, string> EquipMainProps = new();
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
        var fileMap = new Dictionary<string, Dictionary<int, string>>
        {
            ["item.txt"] = Items,
            ["equipment.txt"] = Equips,
            ["equip_main_prop.txt"] = EquipMainProps,
            ["equip_addon_prop.txt"] = EquipAddonProps
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
        //能力
        await LoadAbilityConfigAsync(Path.Combine(dataPath, "ability.txt"));
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
                var classID = int.Parse(tmpTexts[0]);
                targetMap.Add(classID, tmpTexts[1]);
            }
        }
    }
    /// <summary>
    /// 加载能力配置
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task LoadAbilityConfigAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception("未找到配置文件: " + filePath);
        }
        using var streamReader = new StreamReader(filePath, Encoding.UTF8);
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
            tmpTexts = lineContent.Split(lineSeps, 3);
            if (tmpTexts.Length < 3)
            {
            }
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(tmpTexts[i]))
                {
                    return;
                }
            }
            var classID = int.Parse(tmpTexts[0]);
            var maxLevel = int.Parse(tmpTexts[1].Trim());
            var name = tmpTexts[2].Trim();
            Abilities.Add(classID, new AbilityConfig(classID, maxLevel, name));
        }
    }
}
