using ServiceStack.Text;

namespace TextGenerator;

/// <summary>
/// 用于生成txt配置文件的命令行工具
/// 
/// 先解包游戏本地中的pak文件,再使用本工具来生成txt配置文件
/// I:\soft\UnrealPakTool\UnrealPak.exe "F:\SteamLibrary\steamapps\common\The Matchless KungFu\HMS_00\Content\Paks\HMS_00-WindowsNoEditor.pak" -Extract "E:\tmp\hwg" -Filter="*.csv"
/// I:\soft\UnrealPakTool\UnrealPak.exe "F:\SteamLibrary\steamapps\common\The Matchless KungFu\HMS_00\Content\Paks\HMS_00-WindowsNoEditor.pak" -Extract "E:\tmp\hwg" -Filter="*.txt"
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        var etcDirPath = "E:\\tmp\\hwg\\HMS_00\\Content\\Etc";
        GenerateAsync(etcDirPath).Wait();
    }

    private static async Task GenerateAsync(string etcDirPath)
    {
        var dataDirPath = "./data";
        try
        {
            var localizationFilePath = Path.Combine(etcDirPath, "Localization", "ui_zh-Hans.csv");
            var textDictionary = await LoadTextDictionaryAsync(localizationFilePath);
            //创建data目录
            if (!Directory.Exists(dataDirPath))
            {
                Directory.CreateDirectory(dataDirPath);
            }
            //
            await GenerateAbilityAsync(Path.Combine(etcDirPath, "ability.csv"), Path.Combine(dataDirPath, "ability.txt"), textDictionary);
            await GenerateEquipAddonPropAsync(Path.Combine(etcDirPath, "prop.csv"), Path.Combine(dataDirPath, "equip_addon_prop.txt"), textDictionary);
            await GenerateEquipMainPropAsync(Path.Combine(etcDirPath, "equip_main_prop.csv"), Path.Combine(dataDirPath, "equip_main_prop.txt"));
            await GenerateEquipmentAsync(Path.Combine(etcDirPath, "equipment.csv"), Path.Combine(dataDirPath, "equipment.txt"), textDictionary);
            await GenerateItemAsync(Path.Combine(etcDirPath, "item.csv"), Path.Combine(dataDirPath, "item.txt"), textDictionary);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static async Task<Dictionary<string, string>> LoadTextDictionaryAsync(string inputPath)
    {
        using var reader = File.OpenText(inputPath);
        //跳过前面四行
        for (int i = 0; i < 4; i++)
        {
            await reader.ReadLineAsync();
        }
        Dictionary<string, string> textDictionary = new();
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
            {
                if (line is null)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }
            var fields = CsvReader.ParseFields(line);
            var textID = fields[0];
            if (textID is null)
            {
                // ,,
                continue;
            }
            if (!textID.StartsWith('#'))
            {
                var content = fields[1];
                textDictionary[textID] = content;
            }
        }
        return textDictionary;
    }

    private static async Task ConvertCsvAsync(string inputPath, string outputPath, Func<List<string>, string> buildLineFn, string firstLine)
    {
        using var reader = File.OpenText(inputPath);
        //跳过前面四行
        for (int i = 0; i < 4; i++)
        {
            await reader.ReadLineAsync();
        }
        using var writer = File.CreateText(outputPath);
        if (!string.IsNullOrEmpty(firstLine))
        {
            await writer.WriteLineAsync(firstLine);
        }
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
            {
                if (line is null)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }
            var fields = CsvReader.ParseFields(line);
            var textID = fields[0];
            if (textID is null)
            {
                // ,,
                continue;
            }
            if (textID.StartsWith('#'))
            {
                //注释行
                await writer.WriteLineAsync(textID);
                continue;
            }
            var textContent = buildLineFn.Invoke(fields);
            await writer.WriteLineAsync(textContent);
        }
    }

    private static async Task GenerateAbilityAsync(string inputPath, string outputPath, Dictionary<string, string> textDictionary)
    {
        string buildLineFn(List<string> fields)
        {
            var classID = fields[0];
            var maxLevel = fields[9];
            var nameKey = fields[3];
            var name = textDictionary[nameKey];
            return $"{classID} {maxLevel} {name}";
        }
        await ConvertCsvAsync(inputPath, outputPath, buildLineFn, "# 能力Id");
    }
    private static async Task GenerateEquipAddonPropAsync(string inputPath, string outputPath, Dictionary<string, string> textDictionary)
    {
        string buildLineFn(List<string> fields)
        {
            var classID = fields[0];
            var propDescKey = fields[5];
            var propDesc = textDictionary[propDescKey];
            propDesc = propDesc.Replace("<EqptKeyStyle>", string.Empty).Replace("<EqptNumStyle>", string.Empty).Replace("</>", string.Empty);
            var numberType = fields[4];
            var numberTypeText = "+/-";
            if (numberType == "@NUM_TYPE_PERCENT")
            {
                numberTypeText += "%";
            }
            return $"{classID} {propDesc}{numberTypeText}";
        }
        await ConvertCsvAsync(inputPath, outputPath, buildLineFn, "# 属性id");
    }

    private static int ParseEquipType(string equipType)
    {
        var typeNumber = 0;
        switch (equipType)
        {
            case "@EQUIP_TYPE_HAT":
                typeNumber = 0;
                break;
            case "@EQUIP_TYPE_COAT":
                typeNumber = 1;
                break;
            case "@EQUIP_TYPE_PANTS":
                typeNumber = 2;
                break;
            case "@EQUIP_TYPE_WEAPON":
                typeNumber = 3;
                break;
            case "@EQUIP_TYPE_HIDDEN_WEAPON":
                typeNumber = 4;
                break;
        }
        return typeNumber;
    }

    private static async Task GenerateEquipMainPropAsync(string inputPath, string outputPath)
    {
        string buildLineFn(List<string> fields)
        {
            var classID = fields[3];
            var equipType = fields[1];
            var propDesc = fields[2];
            var numberType = fields[4];
            var typeNumber = ParseEquipType(equipType);
            return $"{classID} {typeNumber} {propDesc}";
        }
        await ConvertCsvAsync(inputPath, outputPath, buildLineFn, "# 序列号");
    }
    private static async Task GenerateEquipmentAsync(string inputPath, string outputPath, Dictionary<string, string> textDictionary)
    {
        string buildLineFn(List<string> fields)
        {
            var classID = fields[0];
            var nameKey = fields[1];
            var name = textDictionary[nameKey];
            if (name.StartsWith('「'))
            {
                name = name[1..];
            }
            if (name.EndsWith('」'))
            {
                name = name[..(name.Length-1)];
            }
            var equipType = fields[16];
            var typeNumber = ParseEquipType(equipType);
            return $"{classID} {typeNumber} {name}";
        }
        await ConvertCsvAsync(inputPath, outputPath, buildLineFn, "# 装备编号");
    }
    private static async Task GenerateItemAsync(string inputPath, string outputPath, Dictionary<string, string> textDictionary)
    {
        string buildLineFn(List<string> fields)
        {
            var classID = int.Parse(fields[0]);
            var nameKey = fields[6];
            var name = textDictionary[nameKey];
            if (name.StartsWith('「'))
            {
                name = name[1..];
            }
            if (name.EndsWith('」'))
            {
                name = name[..(name.Length - 1)];
            }
            //藏宝图
            if(classID>= 93003&& classID<= 93005)
            {
                name = fields[3];
            }
            var maxNumber = fields[38];
            return $"{classID} {maxNumber} {name}";
        }
        await ConvertCsvAsync(inputPath, outputPath, buildLineFn, "# 道具编号");
    }
}
