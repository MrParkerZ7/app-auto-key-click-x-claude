using AutoClickKey.Helpers;

namespace AutoClickKey.Models;

public class RecordedAction
{
    public ActionType Type { get; set; }

    public long Timestamp { get; set; }

    public int DelayFromPrevious { get; set; }

    // Mouse properties
    public int X { get; set; }

    public int Y { get; set; }

    public MouseButton MouseButton { get; set; }

    // Keyboard properties
    public ushort KeyCode { get; set; }

    public bool IsKeyDown { get; set; }
}

public enum ActionType
{
    MouseClick,
    MouseMove,
    KeyPress
}
