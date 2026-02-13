using System;

namespace AutoClickKey.Models;

public enum ActionItemType
{
    Click,
    KeyPress,
    Delay
}

public class ActionItem : ICloneable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ActionItemType Type { get; set; } = ActionItemType.Click;
    public bool IsEnabled { get; set; } = true;

    // Click properties
    public int MouseButton { get; set; } // 0=Left, 1=Right, 2=Middle
    public int ClickType { get; set; } // 0=Single, 1=Double
    public bool UseCurrentPosition { get; set; } = true;
    public int FixedX { get; set; }
    public int FixedY { get; set; }

    // Key properties
    public string Key { get; set; } = string.Empty;
    public bool UseCtrl { get; set; }
    public bool UseAlt { get; set; }
    public bool UseShift { get; set; }

    // Common properties
    public int DelayMs { get; set; } = 100;
    public int RepeatCount { get; set; } = 1;

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
            RepeatCount = RepeatCount
        };
    }
}
