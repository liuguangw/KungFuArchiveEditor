using KungFuArchiveEditor.Tools;
using ReactiveUI;

namespace KungFuArchiveEditor.ViewModels;

public class EquipPropViewModel : ViewModelBase
{
    private int id = 0;
    private int propValue = 0;

    public int Id
    {
        get => id;
        set
        {
            if (CheckRaiseAndSetIfChanged(ref id, value))
            {
                this.RaisePropertyChanged(nameof(Description));
            }
        }
    }
    public int Value
    {
        get => propValue;
        set => this.RaiseAndSetIfChanged(ref propValue, value);
    }

    public string Description => GameMetaData.GetItemName(id, GameMetaData.MetaType.EquipAddonProp) ?? "未知属性";
}
