using AutoClickKey.Helpers;

namespace AutoClickKey.Models;

public class ClickerSettings
{
    public MouseButton MouseButton { get; set; } = MouseButton.Left;
    public ClickType ClickType { get; set; } = ClickType.Single;
    public int IntervalMs { get; set; } = 100;
    public PositionMode PositionMode { get; set; } = PositionMode.CurrentPosition;
    public int FixedX { get; set; }
    public int FixedY { get; set; }
    public RepeatMode RepeatMode { get; set; } = RepeatMode.Infinite;
    public int RepeatCount { get; set; } = 10;
}

public enum ClickType
{
    Single,
    Double
}

public enum PositionMode
{
    CurrentPosition,
    FixedPosition
}

public enum RepeatMode
{
    Infinite,
    Count
}
