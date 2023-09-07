using KungFuArchiveEditor.Tools;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;

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
        get => posKey;
        set => posKey = value;
    }
    /// <summary>
    /// 装备名称
    /// </summary>
    public string Name => GameMetaData.GetItemName(classID, GameMetaData.MetaType.Equip) ?? "未知";
    public ObservableCollection<EquipPropViewModel> MainProps { get; } = new();
    public ObservableCollection<EquipPropViewModel> AddonProps { get; } = new();
    public ReactiveCommand<Unit, Unit> AddPropLineCommand { get; }
    //public bool CanAddPropLine => AddonProps.Count < 2;
    public ReactiveCommand<EquipPropViewModel, Unit> DeletePropLineCommand { get; }
    #endregion
    public EquipDialogViewModel()
    {
        //var canCanAddPropLine = this.WhenAnyValue(item => item.CanAddPropLine);
        AddPropLineCommand = ReactiveCommand.Create(AddPropLineAction);
        //AddonProps.CollectionChanged += ReCheckCanAddPropLine;
        DeletePropLineCommand = ReactiveCommand.Create<EquipPropViewModel>(DeletePropLineAction);
    }

    /// <summary>
    /// 重新检测能不能添加附加属性
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /*private void ReCheckCanAddPropLine(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(CanAddPropLine));
    }*/

    public void AddPropLineAction()
    {
        /*var attrIds = new int[] { 101, 120 };
        var currentCount = AddonProps.Count;
        if (currentCount >= attrIds.Length)
        {
            return;
        }*/
        AddonProps.Add(new EquipPropViewModel()
        {
            Id = 101,
            Value = 1
        });
    }

    /// <summary>
    /// 删除一行附加属性
    /// </summary>
    /// <param name="prop"></param>
    private void DeletePropLineAction(EquipPropViewModel prop) => AddonProps.Remove(prop);
}
