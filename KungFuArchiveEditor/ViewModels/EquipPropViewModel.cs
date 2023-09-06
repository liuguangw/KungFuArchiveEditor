using ReactiveUI;

namespace KungFuArchiveEditor.ViewModels;

public class EquipPropViewModel : ViewModelBase
{
    private int id;
    private int propValue;

    public int Id
    {
        get => id;
        set => this.RaiseAndSetIfChanged(ref id, value);
    }
    public int Value
    {
        get => propValue;
        set => this.RaiseAndSetIfChanged(ref propValue, value);
    }
}
