using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KungFuArchiveEditor.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected bool CheckRaiseAndSetIfChanged<T>(
        ref T backingField, T newValue,
        [CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (EqualityComparer<T>.Default.Equals(backingField, newValue))
        {
            return false;
        }

        this.RaisePropertyChanging(propertyName);
        backingField = newValue;
        this.RaisePropertyChanged(propertyName);
        return true;
    }

    protected void RaiseAndSetIfChanged<T>(
        ref T backingField, T newValue, JValue? jsonValue,
        [CallerMemberName] string? propertyName = null)
    {
        if (CheckRaiseAndSetIfChanged(ref backingField, newValue, propertyName))
        {
            if (jsonValue != null)
            {
                jsonValue.Value = newValue;
            }
        }
    }
}
