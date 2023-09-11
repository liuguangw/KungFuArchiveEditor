using Avalonia.Media;
using KungFuArchiveEditor.Assets;
using KungFuArchiveEditor.Tools;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KungFuArchiveEditor.ViewModels;
public class ItemDialogViewModel : ViewModelBase, INotifyDataErrorInfo
{
    #region Fields
    private bool confirmed = false;
    private int classID = 0;
    private string name = string.Empty;
    private int amount = 1;
    private int maxAmount = 1;
    private bool isItem = false;
    private bool isEquip = false;
    private readonly List<ValidationResult> errors = new();
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Properties
    public bool Confirmed
    {
        get => confirmed;
        set => confirmed = value;
    }
    public bool HasErrors => errors.Count > 0;
    public int ClassID
    {
        get => classID;
        set
        {
            if (CheckRaiseAndSetIfChanged(ref classID, value))
            {
                ReloadItemConfig(value);
                this.RaisePropertyChanged(nameof(HasAmount));
                this.RaisePropertyChanged(nameof(NameColor));
                this.RaisePropertyChanged(nameof(CanAdd));
            }
        }
    }

    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public int Amount
    {
        get => amount;
        set
        {
            if (HasErrors)
            {
                errors.Clear();
                RaiseErrorsChanged();
            }
            if (value < 0 || value > maxAmount)
            {
                errors.Add(new ValidationResult(LangResources.Amount + " <= " + maxAmount.ToString()));
                RaiseErrorsChanged();
            }
            this.RaiseAndSetIfChanged(ref amount, value);
        }
    }

    public int MaxAmount
    {
        get => maxAmount;
        set => this.RaiseAndSetIfChanged(ref maxAmount, value);
    }

    /// <summary>
    /// 是否可以修改数量
    /// </summary>
    public bool HasAmount => maxAmount > 1;

    public bool IsItem
    {
        get => isItem;
        set => isItem = value;
    }

    public bool IsEquip
    {
        get => isEquip;
        set => isEquip = value;
    }

    /// <summary>
    /// 名称的颜色显示
    /// </summary>
    public IBrush NameColor
    {
        get
        {
            var isValidClassID = isItem || isEquip;
            return isValidClassID ? Brushes.Black : Brushes.Red;
        }
    }

    /// <summary>
    /// 是否可以执行
    /// </summary>
    public bool CanAdd
    {
        get
        {
            var isValidClassID = isItem || isEquip;
            if (!isValidClassID)
            {
                return false;
            }
            if (amount < 0 || amount > maxAmount)
            {
                return false;
            }
            return true;
        }
    }

    #endregion
    private void ReloadItemConfig(int tClassID)
    {
        if (GameConfigData.Items.TryGetValue(tClassID, out var itemConfig))
        {
            Name = itemConfig.Name;
            MaxAmount = itemConfig.MaxAmount;
            if (amount > maxAmount)
            {
                Amount = maxAmount;
            }
            IsItem = true;
            IsEquip = false;
            return;
        }
        if (GameConfigData.Equips.TryGetValue(tClassID, out var equipConfig))
        {
            Name = equipConfig.Name;
            MaxAmount = 1;
            if (amount > maxAmount)
            {
                Amount = maxAmount;
            }
            IsItem = false;
            IsEquip = true;
            return;
        }
        Name = "未知";
        MaxAmount = 1;
        Amount = 1;
        IsItem = false;
        IsEquip = false;
    }
    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName == nameof(Amount))
        {
            return errors;
        }
        return Array.Empty<ValidationResult>();
    }

    /// <summary>
    /// 通知错误状态更新
    /// </summary>
    private void RaiseErrorsChanged()
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Amount)));
    }
}
