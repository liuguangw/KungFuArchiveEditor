using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

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
        foreach (var itemNode in entityMap)
        {
            var itemJsonData = itemNode.Value;
            if (itemJsonData == null)
            {
                continue;
            }
            var bagItem = new BagItemViewModel() { PosKey = itemNode.Key };
            bagItem.LoadItemData(itemJsonData);
            if (bagItem.EntityType == 5 || bagItem.EntityType == 6)
            {
                continue;
            }
            BagItemList.Add(bagItem);
        }
    }
}
