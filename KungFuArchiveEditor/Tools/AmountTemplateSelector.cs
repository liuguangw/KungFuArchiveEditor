using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using KungFuArchiveEditor.ViewModels;
using System;
using System.Collections.Generic;

namespace KungFuArchiveEditor.Tools;

public class AmountTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();
    public Control? Build(object? param)
    {
        if(param is null)
        {
            throw new ArgumentNullException(nameof(param));
        }
        var key = "default";
        if(param is BagItemEquipViewModel)
        {
            key = "equip";
        }
        return AvailableTemplates[key].Build(param);
    }

    public bool Match(object? data)
    {
        if (data is BagItemViewModel)
        {
            return true;
        }
        return false;
    }
}
