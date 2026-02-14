using System;
using System.Collections.Generic;

namespace AutoClickKey.Models;

public class Profile
{
    public string Name { get; set; } = "Default";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime ModifiedAt { get; set; } = DateTime.Now;

    public List<ActionItem> Actions { get; set; } = new();

    public bool LoopActions { get; set; } = true;

    public int LoopCount { get; set; } = 1; // 0 = infinite

    public int DelayBetweenLoops { get; set; } = 0;

    public int DelayBetweenActions { get; set; } = 0;

    public bool RestoreMousePosition { get; set; } = false;

    // Legacy support
    public ClickerSettings ClickerSettings { get; set; } = new();

    public KeyboardSettings KeyboardSettings { get; set; } = new();

    public List<RecordedAction> RecordedActions { get; set; } = new();

    public HotkeySettings HotkeySettings { get; set; } = new();
}

public class HotkeySettings
{
    public int StartStopKey { get; set; } = 0x75; // F6

    public int EmergencyStopKey { get; set; } = 0x77; // F8

    public int RecordKey { get; set; } = 0x52; // R (with Ctrl)

    public int PlayKey { get; set; } = 0x50; // P (with Ctrl)
}
