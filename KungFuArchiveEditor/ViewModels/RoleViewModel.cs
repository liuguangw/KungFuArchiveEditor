using ReactiveUI;

namespace KungFuArchiveEditor.ViewModels;

public class RoleViewModel : ViewModelBase
{
    #region Fields
    private string userID = string.Empty;
    private string name = string.Empty;
    private int maxHP = 0;
    private int maxZhenqi = 0;
    private int maxBurden = 0;
    #endregion

    #region Properties
    public string UserID
    {
        get => userID;
        set => this.RaiseAndSetIfChanged(ref userID, value);
    }
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }
    public int MaxHP
    {
        get => maxHP;
        set => this.RaiseAndSetIfChanged(ref maxHP, value);
    }
    public int MaxZhenqi
    {
        get => maxZhenqi;
        set => this.RaiseAndSetIfChanged(ref maxZhenqi, value);
    }
    public int MaxBurden
    {
        get => maxBurden;
        set => this.RaiseAndSetIfChanged(ref maxBurden, value);
    }
    #endregion
}
