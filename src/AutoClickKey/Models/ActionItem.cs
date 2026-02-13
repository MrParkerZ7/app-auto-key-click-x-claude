using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoClickKey.Models;

public enum ActionItemType
{
    Click,
    KeyPress,
    Delay
}

public class ActionItem : ICloneable, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private Guid _id = Guid.NewGuid();
    private ActionItemType _type = ActionItemType.Click;
    private bool _isEnabled = true;
    private int _mouseButton;
    private int _clickType;
    private bool _useCurrentPosition = false;
    private int _fixedX;
    private int _fixedY;
    private string _key = string.Empty;
    private bool _useCtrl;
    private bool _useAlt;
    private bool _useShift;
    private int _delayMs = 100;
    private int _repeatCount = 1;
    private string _remark = string.Empty;

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public ActionItemType Type
    {
        get => _type;
        set => SetProperty(ref _type, value, nameof(DisplayName));
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    // Click properties
    public int MouseButton
    {
        get => _mouseButton;
        set => SetProperty(ref _mouseButton, value, nameof(DisplayName));
    }

    public int ClickType
    {
        get => _clickType;
        set => SetProperty(ref _clickType, value);
    }

    public bool UseCurrentPosition
    {
        get => _useCurrentPosition;
        set => SetProperty(ref _useCurrentPosition, value, nameof(DisplayName));
    }

    public int FixedX
    {
        get => _fixedX;
        set => SetProperty(ref _fixedX, value, nameof(DisplayName));
    }

    public int FixedY
    {
        get => _fixedY;
        set => SetProperty(ref _fixedY, value, nameof(DisplayName));
    }

    // Key properties
    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value, nameof(DisplayName));
    }

    public bool UseCtrl
    {
        get => _useCtrl;
        set => SetProperty(ref _useCtrl, value, nameof(DisplayName));
    }

    public bool UseAlt
    {
        get => _useAlt;
        set => SetProperty(ref _useAlt, value, nameof(DisplayName));
    }

    public bool UseShift
    {
        get => _useShift;
        set => SetProperty(ref _useShift, value, nameof(DisplayName));
    }

    // Common properties
    public int DelayMs
    {
        get => _delayMs;
        set => SetProperty(ref _delayMs, value, nameof(DisplayName));
    }

    public int RepeatCount
    {
        get => _repeatCount;
        set => SetProperty(ref _repeatCount, value);
    }

    public string Remark
    {
        get => _remark;
        set => SetProperty(ref _remark, value);
    }

    public string DisplayName => Type switch
    {
        ActionItemType.Click => $"Click {GetMouseButtonName()} at {(UseCurrentPosition ? "cursor" : $"({FixedX}, {FixedY})")}",
        ActionItemType.KeyPress => $"Press {GetKeyDisplayName()}",
        ActionItemType.Delay => $"Wait {DelayMs}ms",
        _ => "Unknown"
    };

    private string GetMouseButtonName() => MouseButton switch
    {
        0 => "Left",
        1 => "Right",
        2 => "Middle",
        _ => "Left"
    };

    private string GetKeyDisplayName()
    {
        var modifiers = "";
        if (UseCtrl) modifiers += "Ctrl+";
        if (UseAlt) modifiers += "Alt+";
        if (UseShift) modifiers += "Shift+";
        return modifiers + Key;
    }

    protected bool SetProperty<T>(ref T field, T value, string? alsoNotify = null, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        if (alsoNotify != null)
        {
            OnPropertyChanged(alsoNotify);
        }
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public object Clone()
    {
        return new ActionItem
        {
            Id = Guid.NewGuid(),
            Type = Type,
            IsEnabled = IsEnabled,
            MouseButton = MouseButton,
            ClickType = ClickType,
            UseCurrentPosition = UseCurrentPosition,
            FixedX = FixedX,
            FixedY = FixedY,
            Key = Key,
            UseCtrl = UseCtrl,
            UseAlt = UseAlt,
            UseShift = UseShift,
            DelayMs = DelayMs,
            RepeatCount = RepeatCount,
            Remark = Remark
        };
    }
}
