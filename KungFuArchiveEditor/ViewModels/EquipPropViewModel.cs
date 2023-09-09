using KungFuArchiveEditor.Tools;
using ReactiveUI;

namespace KungFuArchiveEditor.ViewModels;

public class EquipPropViewModel : ViewModelBase
{
    //private bool isMainProp = false;
    private int id = 0;
    private int propValue = 0;

    //public bool IsMainProp { get => isMainProp; set => isMainProp = value; }
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

    public string Description
    {
        get
        {
            GameConfigData.EquipAddonProps.TryGetValue(id, out string? propName);
            if (propName != null)
            {
                return propName;
            }
            return "未知属性";
        }
    }
}
