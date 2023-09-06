using KungFuArchiveEditor.Tools;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace KungFuArchiveEditor.ViewModels;

public class EquipDialogViewModel : BagViewModel
{
    #region DialogAtrs
    public string Title { get; set; } = "修改装备属性";
    /// <summary>
    /// 是否确定
    /// </summary>
    public bool Confirmed { get; set; } = false;
    #endregion
    #region fields
    private int classID = 0;
    private int rarity = 0;
    private string posKey = "-";
    #endregion
    #region Properties
    /// <summary>
    /// 装备编号
    /// </summary>
    public int ClassID
    {
        get => classID;
        set => this.RaiseAndSetIfChanged(ref classID, value);
    }
    /// <summary>
    /// 品级
    /// </summary>
    public int Rarity
    {
        get => rarity;
        set => this.RaiseAndSetIfChanged(ref rarity, value);
    }
    /// <summary>
    /// 位置(仅用于展示)
    /// </summary>
    public string PosKey
    {
        get=> posKey;
        set=>posKey = value;
    }
    /// <summary>
    /// 装备名称
    /// </summary>
    public string Name => GameMetaData.GetItemName(classID, GameMetaData.MetaType.Equip) ?? "未知";
    public ObservableCollection<EquipPropViewModel> MainProps { get; } = new();
    public ObservableCollection<EquipPropViewModel> AddonProps { get; } = new();
    #endregion
}
