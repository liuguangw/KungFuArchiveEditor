using Newtonsoft.Json.Linq;
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
        var metaDataFilePath = Path.Combine(exeDirPath, "metadata.json");
        if (!File.Exists(metaDataFilePath))
        {
            return;
        }
        using var streamReader = new StreamReader(metaDataFilePath, Encoding.UTF8);
        var fileContent = await streamReader.ReadToEndAsync();
        var jsonData = JObject.Parse(fileContent);
        if (jsonData == null)
        {
            return;
        }
        if (jsonData["item_names"] is JArray itemNameList)
        {
            foreach (var item in itemNameList)
            {
                if (item != null)
                {
                    CheckAddItem(item);
                }
            }
        }
    }

    private static void CheckAddItem(JToken item)
    {
        var classIdObject = item["class_id"];
        var nameObject = item["name"];
        if ((classIdObject == null) || (nameObject == null))
        {
            return;
        }
        if (classIdObject.Type == JTokenType.Integer && nameObject.Type == JTokenType.String)
        {
            var classID = classIdObject.ToObject<int>();
            var name = nameObject.ToObject<string>()!;
            itemNames.Add(classID, name);
        }
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
