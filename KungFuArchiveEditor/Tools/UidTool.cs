using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.Tools;
public static class UidTool
{
    private static long gameMaxUid = 0;
    public static long NextUid()
    {
        if (gameMaxUid == 0)
        {
            throw new Exception("uid未初始化");
        }
        gameMaxUid++;
        return gameMaxUid;
    }

    public static async Task InitToolAsync(JObject shareWorldData, IStorageFolder saveFolder)
    {
        gameMaxUid = 0;
        var playersDataObject = shareWorldData["common_players_data"];
        if (playersDataObject is JObject commonPlayersData)
        {
            foreach (var playerProperty in commonPlayersData)
            {
                var playerUid = playerProperty.Key;
                if (playerUid == null)
                {
                    continue;
                }
                var playerData = playerProperty.Value;
                if (playerData == null)
                {
                    continue;
                }
                var componentMaxUid = GetPlayerComponentMaxUid(playerData);
                if (componentMaxUid > gameMaxUid)
                {
                    gameMaxUid = componentMaxUid;
                }
            }
        }
        var folderPath = saveFolder.TryGetLocalPath();
        if (folderPath == null)
        {
            return;
        }
        var di = new DirectoryInfo(folderPath);
        foreach (var file in di.GetFiles("world_data.*", SearchOption.AllDirectories))
        {
            try
            {
                var worldMaxUid = await GetWorldMaxUidAsync(file);
                if (worldMaxUid > gameMaxUid)
                {
                    gameMaxUid = worldMaxUid;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }
    }

    private static long GetPlayerComponentMaxUid(JToken playerData)
    {
        long maxUid = 0;
        var playerUidObject = playerData["uid"];
        if (playerUidObject != null)
        {
            var playerUidText = playerUidObject.ToObject<string>();
            var playerUid = ParseUid(playerUidText);
            if (playerUid > maxUid)
            {
                maxUid = playerUid;
            }
        }
        var componentKeyList = new string[]
        {
            "container","neigong_container","ability_container",
            "task_container","destiny_container"
        };
        foreach (var componentKey in componentKeyList)
        {
            var entityMapObject = playerData.SelectToken($"component_data.{componentKey}.entity_map");
            if (entityMapObject is JObject entityMap)
            {
                foreach (var itemProperty in entityMap)
                {
                    var uid = GetMapMaxUid(entityMap);
                    if (uid > maxUid)
                    {
                        maxUid = uid;
                    }
                }
            }
        }
        return maxUid;
    }

    private static async Task<long> GetWorldMaxUidAsync(FileInfo file)
    {
        long maxUid = 0;
        var jsonData = await ArchiveTool.LoadArchiveAsync(file);
        if (jsonData == null)
        {
            return maxUid;
        }
        var commonKeyList = new string[]
        {
            "tile_map","decoration_map","sect_map","map_tag_map",
            "family_map","activity_map"
        };
        foreach (var commonKey in commonKeyList)
        {
            var mapObject = jsonData[commonKey];
            if (mapObject is JObject entityMap)
            {
                var uid = GetMapMaxUid(entityMap);
                if (uid > maxUid)
                {
                    maxUid = uid;
                }
            }
        }
        //mail_system
        {
            var mapObject = jsonData.SelectToken("mail_system.mail_map");
            if (mapObject is JObject entityMap)
            {
                var uid = GetMapMaxUid(entityMap, "mail_uid");
                if (uid > maxUid)
                {
                    maxUid = uid;
                }
            }
        }
        //message_system
        {
            var mapObject = jsonData.SelectToken("message_system.message_map");
            if (mapObject is JObject entityMap)
            {
                var uid = GetMapMaxUid(entityMap);
                if (uid > maxUid)
                {
                    maxUid = uid;
                }
            }
        }
        //npc
        {
            var mapObject = jsonData["npc_map"];
            if (mapObject is JObject entityMap)
            {
                var uid = GetPlayerComponentMaxUid(mapObject);
                if (uid > maxUid)
                {
                    maxUid = uid;
                }
            }
        }
        //monster
        {
            var mapObject = jsonData["monster_map"];
            if (mapObject is JObject entityMap)
            {
                var uid = GetPlayerComponentMaxUid(mapObject);
                if (uid > maxUid)
                {
                    maxUid = uid;
                }
            }
        }
        //
        return maxUid;
    }

    private static long GetMapMaxUid(JObject entityMap, string field = "uid")
    {
        long maxUid = 0;
        foreach (var itemProperty in entityMap)
        {
            var itemNode = itemProperty.Value;
            if (itemNode == null)
            {
                continue;
            }
            var uidObject = itemNode[field];
            if (uidObject == null)
            {
                continue;
            }
            var uidText = uidObject.ToObject<string>();
            var uid = ParseUid(uidText);
            if (uid > maxUid)
            {
                maxUid = uid;
            }
        }
        return maxUid;
    }

    /// <summary>
    /// uid解析
    /// </summary>
    /// <param name="uidText"></param>
    /// <returns></returns>
    private static long ParseUid(string? uidText)
    {
        if (!string.IsNullOrEmpty(uidText))
        {
            if (long.TryParse(uidText, out long uid))
            {
                return uid;
            }
        }
        return 0;
    }
}
