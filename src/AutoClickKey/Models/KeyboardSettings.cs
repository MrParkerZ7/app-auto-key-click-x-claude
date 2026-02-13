namespace AutoClickKey.Models;

public class KeyboardSettings
{
    public KeyboardMode Mode { get; set; } = KeyboardMode.TypeText;

    public string TextToType { get; set; } = string.Empty;

    public ushort KeyCode { get; set; }

    public bool UseCtrl { get; set; }

    public bool UseAlt { get; set; }

    public bool UseShift { get; set; }

    public int IntervalMs { get; set; } = 50;

    public RepeatMode RepeatMode { get; set; } = RepeatMode.Infinite;

    public int RepeatCount { get; set; } = 10;
}

public enum KeyboardMode
{
    TypeText,
    PressKey
}
