using System;
using System.Windows;
using System.Windows.Input;
using AutoClickKey.Helpers;
using AutoClickKey.Models;
using AutoClickKey.Services;

namespace AutoClickKey.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ClickerService _clickerService;
    private readonly KeyboardService _keyboardService;
    private readonly RecorderService _recorderService;
    private readonly ProfileService _profileService;
    private readonly HotkeyService _hotkeyService;

    private bool _isRunning;
    private bool _isRecording;
    private int _clickCount;
    private int _keyCount;
    private string _statusText = "Ready - Press F6 to Start";

    #region Properties

    // Auto Clicker Settings
    private int _selectedMouseButton;
    public int SelectedMouseButton
    {
        get => _selectedMouseButton;
        set => SetProperty(ref _selectedMouseButton, value);
    }

    private int _selectedClickType;
    public int SelectedClickType
    {
        get => _selectedClickType;
        set => SetProperty(ref _selectedClickType, value);
    }

    private int _clickInterval = 100;
    public int ClickInterval
    {
        get => _clickInterval;
        set => SetProperty(ref _clickInterval, Math.Max(1, value));
    }

    private bool _useCurrentPosition = true;
    public bool UseCurrentPosition
    {
        get => _useCurrentPosition;
        set => SetProperty(ref _useCurrentPosition, value);
    }

    private int _fixedX;
    public int FixedX
    {
        get => _fixedX;
        set => SetProperty(ref _fixedX, value);
    }

    private int _fixedY;
    public int FixedY
    {
        get => _fixedY;
        set => SetProperty(ref _fixedY, value);
    }

    private bool _clickInfinite = true;
    public bool ClickInfinite
    {
        get => _clickInfinite;
        set => SetProperty(ref _clickInfinite, value);
    }

    private int _clickRepeatCount = 10;
    public int ClickRepeatCount
    {
        get => _clickRepeatCount;
        set => SetProperty(ref _clickRepeatCount, Math.Max(1, value));
    }

    // Auto Keyboard Settings
    private int _selectedKeyboardMode;
    public int SelectedKeyboardMode
    {
        get => _selectedKeyboardMode;
        set => SetProperty(ref _selectedKeyboardMode, value);
    }

    private string _textToType = string.Empty;
    public string TextToType
    {
        get => _textToType;
        set => SetProperty(ref _textToType, value);
    }

    private string _selectedKey = string.Empty;
    public string SelectedKey
    {
        get => _selectedKey;
        set => SetProperty(ref _selectedKey, value);
    }

    private bool _useCtrl;
    public bool UseCtrl
    {
        get => _useCtrl;
        set => SetProperty(ref _useCtrl, value);
    }

    private bool _useAlt;
    public bool UseAlt
    {
        get => _useAlt;
        set => SetProperty(ref _useAlt, value);
    }

    private bool _useShift;
    public bool UseShift
    {
        get => _useShift;
        set => SetProperty(ref _useShift, value);
    }

    private int _keyInterval = 50;
    public int KeyInterval
    {
        get => _keyInterval;
        set => SetProperty(ref _keyInterval, Math.Max(1, value));
    }

    private bool _keyInfinite = true;
    public bool KeyInfinite
    {
        get => _keyInfinite;
        set => SetProperty(ref _keyInfinite, value);
    }

    private int _keyRepeatCount = 10;
    public int KeyRepeatCount
    {
        get => _keyRepeatCount;
        set => SetProperty(ref _keyRepeatCount, Math.Max(1, value));
    }

    // Status
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (SetProperty(ref _isRunning, value))
            {
                UpdateStatusText();
            }
        }
    }

    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (SetProperty(ref _isRecording, value))
            {
                UpdateStatusText();
            }
        }
    }

    public int ClickCount
    {
        get => _clickCount;
        set => SetProperty(ref _clickCount, value);
    }

    public int KeyCount
    {
        get => _keyCount;
        set => SetProperty(ref _keyCount, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    // Active Tab
    private int _selectedTab;
    public int SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    #endregion

    #region Commands

    public ICommand ToggleCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RecordCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand PickPositionCommand { get; }

    #endregion

    public MainViewModel()
    {
        _clickerService = new ClickerService();
        _keyboardService = new KeyboardService();
        _recorderService = new RecorderService();
        _profileService = new ProfileService();
        _hotkeyService = new HotkeyService();

        // Setup event handlers
        _clickerService.ClickPerformed += (_, count) =>
            Application.Current.Dispatcher.Invoke(() => ClickCount = count);
        _clickerService.Stopped += (_, _) =>
            Application.Current.Dispatcher.Invoke(() => IsRunning = false);

        _keyboardService.KeyPressed += (_, count) =>
            Application.Current.Dispatcher.Invoke(() => KeyCount = count);
        _keyboardService.Stopped += (_, _) =>
            Application.Current.Dispatcher.Invoke(() => IsRunning = false);

        _recorderService.RecordingStopped += (_, _) =>
            Application.Current.Dispatcher.Invoke(() => IsRecording = false);

        // Setup commands
        ToggleCommand = new RelayCommand(Toggle);
        StopCommand = new RelayCommand(Stop);
        RecordCommand = new RelayCommand(ToggleRecording);
        PlayCommand = new RelayCommand(PlayRecording);
        PickPositionCommand = new RelayCommand(PickPosition);
    }

    public void InitializeHotkeys(Window window)
    {
        _hotkeyService.Initialize(window);

        // F6 - Start/Stop
        _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, 0x75, Toggle);

        // F8 - Emergency Stop
        _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, 0x77, EmergencyStop);
    }

    private void Toggle()
    {
        if (IsRunning)
        {
            Stop();
        }
        else
        {
            Start();
        }
    }

    private async void Start()
    {
        IsRunning = true;
        ClickCount = 0;
        KeyCount = 0;

        // Start based on selected tab
        if (SelectedTab == 0) // Auto Clicker
        {
            var settings = new ClickerSettings
            {
                MouseButton = (Helpers.MouseButton)SelectedMouseButton,
                ClickType = (ClickType)SelectedClickType,
                IntervalMs = ClickInterval,
                PositionMode = UseCurrentPosition ? PositionMode.CurrentPosition : PositionMode.FixedPosition,
                FixedX = FixedX,
                FixedY = FixedY,
                RepeatMode = ClickInfinite ? RepeatMode.Infinite : RepeatMode.Count,
                RepeatCount = ClickRepeatCount
            };
            await _clickerService.StartAsync(settings);
        }
        else if (SelectedTab == 1) // Auto Keyboard
        {
            var settings = new KeyboardSettings
            {
                Mode = (KeyboardMode)SelectedKeyboardMode,
                TextToType = TextToType,
                KeyCode = ParseKeyCode(SelectedKey),
                UseCtrl = UseCtrl,
                UseAlt = UseAlt,
                UseShift = UseShift,
                IntervalMs = KeyInterval,
                RepeatMode = KeyInfinite ? RepeatMode.Infinite : RepeatMode.Count,
                RepeatCount = KeyRepeatCount
            };
            await _keyboardService.StartAsync(settings);
        }
    }

    private void Stop()
    {
        _clickerService.Stop();
        _keyboardService.Stop();
        IsRunning = false;
    }

    private void EmergencyStop()
    {
        Stop();
        _recorderService.StopRecording();
        _recorderService.StopPlayback();
        IsRecording = false;
    }

    private void ToggleRecording()
    {
        if (IsRecording)
        {
            _recorderService.StopRecording();
            IsRecording = false;
        }
        else
        {
            _recorderService.StartRecording();
            IsRecording = true;
        }
    }

    private async void PlayRecording()
    {
        if (_recorderService.IsPlaying)
        {
            _recorderService.StopPlayback();
        }
        else
        {
            await _recorderService.PlayAsync();
        }
    }

    private void PickPosition()
    {
        var position = Win32Api.GetCursorPosition();
        FixedX = position.X;
        FixedY = position.Y;
    }

    private void UpdateStatusText()
    {
        if (IsRecording)
        {
            StatusText = "Recording... Press F8 to Stop";
        }
        else if (IsRunning)
        {
            StatusText = "Running... Press F6 to Stop";
        }
        else
        {
            StatusText = "Ready - Press F6 to Start";
        }
    }

    private static ushort ParseKeyCode(string key)
    {
        if (string.IsNullOrEmpty(key)) return 0;

        // Try to parse as virtual key code
        if (key.Length == 1)
        {
            return (ushort)char.ToUpper(key[0]);
        }

        // Handle special keys
        return key.ToUpper() switch
        {
            "ENTER" => 0x0D,
            "TAB" => 0x09,
            "SPACE" => 0x20,
            "BACKSPACE" => 0x08,
            "DELETE" => 0x2E,
            "ESCAPE" or "ESC" => 0x1B,
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            _ => 0
        };
    }

    public void Dispose()
    {
        _hotkeyService.Dispose();
    }
}
