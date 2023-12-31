using KungFuArchiveEditor.Assets;
using KungFuArchiveEditor.GameConfig;
using KungFuArchiveEditor.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KungFuArchiveEditor.ViewModels;

public class AbilityItemViewModel : ViewModelBase, INotifyDataErrorInfo
{
    #region Fields
    private int classID = 0;
    private int level = 0;
    private int exp = 0;

    protected JValue? levelObject = null;
    protected JValue? expObject = null;

    private readonly List<ValidationResult> errors = new();
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Properties
    public bool HasErrors => errors.Count > 0;
    public int ClassID => classID;

    public string Name => GetName(classID);
    public int Level
    {
        get => level;
        set
        {
            if (HasErrors)
            {
                errors.Clear();
                RaiseErrorsChanged();
            }
            if (value < 0 || value > MaxLevel)
            {
                errors.Add(new ValidationResult(LangResources.Level + " <= " + MaxLevel.ToString()));
                RaiseErrorsChanged();
            }
            RaiseAndSetIfChanged(ref level, value, levelObject);
        }
    }
    public int Exp
    {
        get => exp;
        set => RaiseAndSetIfChanged(ref exp, value, expObject);
    }

    public int MaxLevel => GetMaxLevel(classID);

    #endregion

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName == nameof(Level))
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
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Level)));
    }

    private static string GetName(int classID)
    {
        GameConfigData.Abilities.TryGetValue(classID, out AbilityConfig? abilityConfig);
        if (abilityConfig != null)
        {
            return abilityConfig.Name;
        }
        return "未知";
    }
    private static int GetMaxLevel(int classID)
    {
        GameConfigData.Abilities.TryGetValue(classID, out AbilityConfig? abilityConfig);
        if (abilityConfig != null)
        {
            return abilityConfig.MaxLevel;
        }
        return 6;
    }

    public void LoadData(JToken jsonData)
    {
        //class_id
        var objectNode = jsonData["class_id"];
        if (objectNode != null)
        {
            classID = objectNode.ToObject<int>();
        }
        //level
        objectNode = jsonData["level"];
        if (objectNode != null)
        {
            level = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                levelObject = value;
            }
        }
        //exp
        objectNode = jsonData["exp"];
        if (objectNode != null)
        {
            exp = objectNode.ToObject<int>();
            if (objectNode is JValue value)
            {
                expObject = value;
            }
        }
    }
}
