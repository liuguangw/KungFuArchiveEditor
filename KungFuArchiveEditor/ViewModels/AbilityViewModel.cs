using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace KungFuArchiveEditor.ViewModels;

public class AbilityViewModel : ViewModelBase
{
    #region Properties
    public ObservableCollection<AbilityItemViewModel> ItemList { get; } = new();
    #endregion
    /// <summary>
    /// 从json对象中加载能力列表
    /// </summary>
    /// <param name="entityMap"></param>
    public void LoadItemList(JObject entityMap)
    {
        ItemList.Clear();
        var itemNodes = entityMap.Properties();
        //
        AbilityItemViewModel item;
        foreach (var itemNodeProperty in itemNodes)
        {
            if (itemNodeProperty == null)
            {
                continue;
            }
            item = new AbilityItemViewModel();
            item.LoadData(itemNodeProperty.Value);
            ItemList.Add(item);
        }
    }
}
