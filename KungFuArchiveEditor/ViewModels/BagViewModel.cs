using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Linq;

namespace KungFuArchiveEditor.ViewModels;
public class BagViewModel : ViewModelBase
{
    #region Properties
    public ObservableCollection<BagItemViewModel> BagItemList { get; } = new();
    #endregion
    /// <summary>
    /// 从json对象中加载物品列表
    /// </summary>
    /// <param name="entityMap"></param>
    public void LoadBagItemList(JObject entityMap)
    {
        BagItemList.Clear();
        var itemNodes = entityMap.Properties()
            .Where(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                var mainPos = posArr[0];
                return mainPos == 2 || mainPos == 4;
            })
            //道具在前面
            .OrderByDescending(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                return posArr[0];
            })
            //按位置排序
            .ThenBy(itemProperty =>
            {
                var posArr = ParsePos(itemProperty.Name);
                return posArr[2];
            });
        //
        foreach (var itemNodeProperty in itemNodes)
        {
            if (itemNodeProperty == null)
            {
                continue;
            }
            BagItemList.Add(ParseBagItem(itemNodeProperty.Name, itemNodeProperty.Value));
        }
    }

    private static int[] ParsePos(string posKey)
    {
        var posArr = new int[] { 0, 0, 0 };
        //解析pos
        var numTexts = posKey.Split('-');
        if (numTexts.Length == 3)
        {
            posArr[0] = int.Parse(numTexts[0]);
            posArr[1] = int.Parse(numTexts[1]);
            posArr[2] = int.Parse(numTexts[2]);
        }

        return posArr;
    }

    private static BagItemViewModel ParseBagItem(string posKey, JToken itemJsonData)
    {
        var entityType = 0;
        var objectNode = itemJsonData["entity_type"];
        if (objectNode != null)
        {
            entityType = objectNode.ToObject<int>();
        }
        //
        var posArr = ParsePos(posKey);
        BagItemViewModel bagItem;
        if(entityType == 5)
        {
            //装备
            bagItem = new BagItemEquipViewModel();
        }
        else
        {
            bagItem = new BagItemViewModel();
        }
        bagItem.LoadItemData(posArr, entityType, itemJsonData);
        return bagItem;
    }
}
