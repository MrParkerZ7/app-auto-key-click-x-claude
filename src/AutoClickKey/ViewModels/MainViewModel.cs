using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AutoClickKey.Helpers;
using AutoClickKey.Models;
using AutoClickKey.Services;

namespace AutoClickKey.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ActionRunnerService _actionRunner;
    private readonly ProfileService _profileService;
    private readonly HotkeyService _hotkeyService;
    private readonly PositionPickerService _positionPicker;
    private readonly SettingsService _settingsService;
    private readonly DispatcherTimer _saveDebounceTimer;
    private Window? _mainWindow;
    private bool _isLoading; // Prevent auto-save during load
    private string? _originalProfileName; // Track loaded profile name for rename

    public MainViewModel()
    {
        _actionRunner = new ActionRunnerService();
        _profileService = new ProfileService();
        _hotkeyService = new HotkeyService();
        _positionPicker = new PositionPickerService();
        _settingsService = new SettingsService();

        // Setup debounce timer for auto-save (500ms delay)
        _saveDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _saveDebounceTimer.Tick += (_, _) =>
        {
            _saveDebounceTimer.Stop();
            PerformAutoSave();
        };

        // Setup position picker events
        _positionPicker.PositionPicked += (_, pos) =>
            Application.Current.Dispatcher.Invoke(() => OnPositionPicked(pos.X, pos.Y));
        _positionPicker.PickingCancelled += (_, _) =>
            Application.Current.Dispatcher.Invoke(() => OnPickingCancelled());

        Actions = new ObservableCollection<ActionItem>();
        Actions.CollectionChanged += (_, _) => AutoSave();
        ProfileNames = new ObservableCollection<string>();

        // Setup event handlers
        _actionRunner.ActionExecuted += (_, e) =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentActionIndex = e.ActionIndex;
                CurrentLoopCount = e.LoopCount;
            });
        _actionRunner.Stopped += (_, _) =>
            Application.Current.Dispatcher.Invoke(() => IsRunning = false);

        // Setup commands
        ToggleCommand = new RelayCommand(Toggle);
        AddClickCommand = new RelayCommand(AddClickAction);
        AddKeyCommand = new RelayCommand(AddKeyAction);
        AddDelayCommand = new RelayCommand(AddDelayAction);
        RemoveActionCommand = new RelayCommand(RemoveSelectedAction, () => SelectedAction != null);
        MoveUpCommand = new RelayCommand(MoveActionUp, () => SelectedAction != null && Actions.IndexOf(SelectedAction) > 0);
        MoveDownCommand = new RelayCommand(MoveActionDown, () => SelectedAction != null && Actions.IndexOf(SelectedAction) < Actions.Count - 1);
        DuplicateCommand = new RelayCommand(DuplicateAction, () => SelectedAction != null);
        ClearAllCommand = new RelayCommand(ClearAllActions, () => Actions.Count > 0);
        DeleteProfileCommand = new RelayCommand(DeleteSelectedProfile, () => !string.IsNullOrEmpty(SelectedProfileName));
        NewProfileCommand = new RelayCommand(NewProfile);
        PickPositionCommand = new RelayCommand(PickPosition);

        // Load profile names
        RefreshProfileList();

        // Load settings and last profile
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Load();

        // Restore hotkey
        if (!string.IsNullOrEmpty(settings.LastHotkey) && AvailableHotkeys.Contains(settings.LastHotkey))
        {
            _selectedHotkey = settings.LastHotkey;
        }

        // Load last profile
        if (!string.IsNullOrEmpty(settings.LastProfileName) && ProfileNames.Contains(settings.LastProfileName))
        {
            LoadProfile(settings.LastProfileName);
            _selectedProfileName = settings.LastProfileName;
            OnPropertyChanged(nameof(SelectedProfileName));
        }
        else
        {
            // Start with empty new profile
            AddClickAction();
        }
    }

    public void SaveSettings()
    {
        // Flush any pending auto-save
        if (_saveDebounceTimer.IsEnabled)
        {
            _saveDebounceTimer.Stop();
            PerformAutoSave();
        }

        var settings = new AppSettings
        {
            LastProfileName = string.IsNullOrWhiteSpace(CurrentProfileName) ? null : CurrentProfileName,
            LastHotkey = SelectedHotkey
        };
        _settingsService.Save(settings);
    }

    #region Properties

    public ObservableCollection<ActionItem> Actions { get; }
    public ObservableCollection<string> ProfileNames { get; }

    private ActionItem? _selectedAction;
    public ActionItem? SelectedAction
    {
        get => _selectedAction;
        set
        {
            if (SetProperty(ref _selectedAction, value))
            {
                OnPropertyChanged(nameof(HasSelectedAction));
                OnPropertyChanged(nameof(IsClickAction));
                OnPropertyChanged(nameof(IsKeyAction));
                OnPropertyChanged(nameof(IsDelayAction));
                (RemoveActionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DuplicateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasSelectedAction => SelectedAction != null;
    public bool IsClickAction => SelectedAction?.Type == ActionItemType.Click;
    public bool IsKeyAction => SelectedAction?.Type == ActionItemType.KeyPress;
    public bool IsDelayAction => SelectedAction?.Type == ActionItemType.Delay;

    private string _currentProfileName = "";
    public string CurrentProfileName
    {
        get => _currentProfileName;
        set
        {
            if (SetProperty(ref _currentProfileName, value))
            {
                AutoSave();
            }
        }
    }

    private string? _selectedProfileName;
    public string? SelectedProfileName
    {
        get => _selectedProfileName;
        set
        {
            if (SetProperty(ref _selectedProfileName, value))
            {
                (DeleteProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    LoadProfile(value);
                }
            }
        }
    }

    private bool _loopActions = true;
    public bool LoopActions
    {
        get => _loopActions;
        set { if (SetProperty(ref _loopActions, value)) AutoSave(); }
    }

    private int _loopCount = 10;
    public int LoopCount
    {
        get => _loopCount;
        set { if (SetProperty(ref _loopCount, Math.Max(0, value))) AutoSave(); }
    }

    private int _delayBetweenLoops;
    public int DelayBetweenLoops
    {
        get => _delayBetweenLoops;
        set { if (SetProperty(ref _delayBetweenLoops, Math.Max(0, value))) AutoSave(); }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (SetProperty(ref _isRunning, value))
            {
                UpdateStatusText();
                OnPropertyChanged(nameof(ToggleButtonText));
            }
        }
    }

    public string ToggleButtonText => IsRunning ? $"Stop ({SelectedHotkey})" : $"Start ({SelectedHotkey})";

    private int _currentActionIndex;
    public int CurrentActionIndex
    {
        get => _currentActionIndex;
        set => SetProperty(ref _currentActionIndex, value);
    }

    private int _currentLoopCount;
    public int CurrentLoopCount
    {
        get => _currentLoopCount;
        set => SetProperty(ref _currentLoopCount, value);
    }

    private string _statusText = "Ready - Press F4 to Start";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private bool _isPickingPosition;
    public bool IsPickingPosition
    {
        get => _isPickingPosition;
        set
        {
            if (SetProperty(ref _isPickingPosition, value))
            {
                UpdateStatusText();
            }
        }
    }

    private string _selectedHotkey = "F4";
    public string SelectedHotkey
    {
        get => _selectedHotkey;
        set
        {
            if (SetProperty(ref _selectedHotkey, value))
            {
                RegisterToggleHotkey();
                UpdateStatusText();
                OnPropertyChanged(nameof(ToggleButtonText));
            }
        }
    }

    public string[] AvailableHotkeys { get; } = ["F1", "F2", "F3", "F4", "F5", "F6", "F7", "F9", "F10", "F11", "F12"];

    #endregion

    #region Commands

    public ICommand ToggleCommand { get; }
    public ICommand AddClickCommand { get; }
    public ICommand AddKeyCommand { get; }
    public ICommand AddDelayCommand { get; }
    public ICommand RemoveActionCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand DuplicateCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand NewProfileCommand { get; }
    public ICommand PickPositionCommand { get; }

    #endregion

    #region Hotkey Setup

    private int _toggleHotkeyId;

    public void InitializeHotkeys(Window window)
    {
        _mainWindow = window;
        _hotkeyService.Initialize(window);

        // Register toggle hotkey
        RegisterToggleHotkey();

        // Escape - Cancel picking
        _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, 0x1B, CancelPicking);
    }

    private void RegisterToggleHotkey()
    {
        if (_mainWindow == null) return;

        // Unregister previous hotkey
        if (_toggleHotkeyId > 0)
        {
            _hotkeyService.UnregisterHotkey(_toggleHotkeyId);
        }

        // Register new hotkey
        var keyCode = GetKeyCode(SelectedHotkey);
        _toggleHotkeyId = _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, keyCode, Toggle);
    }

    private static int GetKeyCode(string key) => key switch
    {
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
        _ => 0x73 // Default to F4
    };

    private void CancelPicking()
    {
        if (IsPickingPosition)
        {
            _positionPicker.CancelPicking();
        }
    }

    #endregion

    #region Action Methods

    private void AddClickAction()
    {
        var action = new ActionItem
        {
            Type = ActionItemType.Click,
            MouseButton = 0,
            ClickType = 0,
            DelayMs = 100
        };
        Actions.Add(action);
        SelectedAction = action;
        (ClearAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void AddKeyAction()
    {
        var action = new ActionItem
        {
            Type = ActionItemType.KeyPress,
            Key = "A",
            DelayMs = 100
        };
        Actions.Add(action);
        SelectedAction = action;
        (ClearAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void AddDelayAction()
    {
        var action = new ActionItem
        {
            Type = ActionItemType.Delay,
            DelayMs = 500
        };
        Actions.Add(action);
        SelectedAction = action;
        (ClearAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void RemoveSelectedAction()
    {
        if (SelectedAction == null) return;

        var index = Actions.IndexOf(SelectedAction);
        Actions.Remove(SelectedAction);

        if (Actions.Count > 0)
        {
            SelectedAction = Actions[Math.Min(index, Actions.Count - 1)];
        }
        else
        {
            SelectedAction = null;
        }
        (ClearAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void MoveActionUp()
    {
        if (SelectedAction == null) return;

        var index = Actions.IndexOf(SelectedAction);
        if (index > 0)
        {
            Actions.Move(index, index - 1);
        }
        (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void MoveActionDown()
    {
        if (SelectedAction == null) return;

        var index = Actions.IndexOf(SelectedAction);
        if (index < Actions.Count - 1)
        {
            Actions.Move(index, index + 1);
        }
        (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void DuplicateAction()
    {
        if (SelectedAction == null) return;

        var clone = (ActionItem)SelectedAction.Clone();
        var index = Actions.IndexOf(SelectedAction);
        Actions.Insert(index + 1, clone);
        SelectedAction = clone;
    }

    private void ClearAllActions()
    {
        Actions.Clear();
        SelectedAction = null;
        (ClearAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void PickPosition()
    {
        if (SelectedAction == null || SelectedAction.Type != ActionItemType.Click) return;
        if (IsPickingPosition) return;

        IsPickingPosition = true;

        // Minimize window so user can click anywhere
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Minimized;
        }

        _positionPicker.StartPicking();
    }

    private void OnPositionPicked(int x, int y)
    {
        IsPickingPosition = false;

        // Restore window
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        if (SelectedAction != null && SelectedAction.Type == ActionItemType.Click)
        {
            SelectedAction.FixedX = x;
            SelectedAction.FixedY = y;
            SelectedAction.UseCurrentPosition = false;
            OnPropertyChanged(nameof(SelectedAction));
        }
    }

    private void OnPickingCancelled()
    {
        IsPickingPosition = false;

        // Restore window
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }
    }

    #endregion

    #region Run Methods

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

    private void Start()
    {
        if (Actions.Count == 0) return;

        IsRunning = true;
        CurrentActionIndex = 0;
        CurrentLoopCount = 0;

        var enabledActions = Actions.Where(a => a.IsEnabled).ToList();
        _ = _actionRunner.RunAsync(enabledActions, LoopActions, LoopCount, DelayBetweenLoops);
    }

    private void Stop()
    {
        _actionRunner.Stop();
        IsRunning = false;
    }

    private void UpdateStatusText()
    {
        if (IsPickingPosition)
        {
            StatusText = "Click anywhere to pick position (Esc to cancel)";
        }
        else if (IsRunning)
        {
            StatusText = $"Running... Loop {CurrentLoopCount} - Press {SelectedHotkey} to Stop";
        }
        else
        {
            StatusText = $"Ready - Press {SelectedHotkey} to Start";
        }
    }

    #endregion

    #region Profile Methods

    private void RefreshProfileList()
    {
        var currentSelection = _selectedProfileName;
        ProfileNames.Clear();
        foreach (var name in _profileService.GetAllProfileNames())
        {
            ProfileNames.Add(name);
        }
        // Restore selection without triggering load
        _selectedProfileName = currentSelection;
        OnPropertyChanged(nameof(SelectedProfileName));
    }

    private void AutoSave()
    {
        if (_isLoading) return;

        // Reset and start debounce timer
        _saveDebounceTimer.Stop();
        _saveDebounceTimer.Start();
    }

    private void PerformAutoSave()
    {
        if (_isLoading) return;
        if (string.IsNullOrWhiteSpace(CurrentProfileName)) return;

        // If renaming an existing profile, delete the old one first
        if (!string.IsNullOrEmpty(_originalProfileName) &&
            _originalProfileName != CurrentProfileName &&
            ProfileNames.Contains(_originalProfileName))
        {
            _profileService.DeleteProfile(_originalProfileName);
        }

        var profile = new Profile
        {
            Name = CurrentProfileName,
            Actions = Actions.ToList(),
            LoopActions = LoopActions,
            LoopCount = LoopCount,
            DelayBetweenLoops = DelayBetweenLoops,
            ModifiedAt = DateTime.Now
        };

        _profileService.SaveProfile(profile);
        _originalProfileName = CurrentProfileName; // Update original name after save
        RefreshProfileList();

        // Update selected profile to match current
        if (_selectedProfileName != CurrentProfileName)
        {
            _selectedProfileName = CurrentProfileName;
            OnPropertyChanged(nameof(SelectedProfileName));
        }
    }

    private void LoadProfile(string name)
    {
        var profile = _profileService.LoadProfile(name);
        if (profile == null) return;

        _isLoading = true;
        try
        {
            Actions.Clear();
            foreach (var action in profile.Actions)
            {
                Actions.Add(action);
            }

            _currentProfileName = profile.Name;
            _originalProfileName = profile.Name; // Track for rename
            OnPropertyChanged(nameof(CurrentProfileName));
            _loopActions = profile.LoopActions;
            OnPropertyChanged(nameof(LoopActions));
            _loopCount = profile.LoopCount;
            OnPropertyChanged(nameof(LoopCount));
            _delayBetweenLoops = profile.DelayBetweenLoops;
            OnPropertyChanged(nameof(DelayBetweenLoops));

            if (Actions.Count > 0)
            {
                SelectedAction = Actions[0];
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void DeleteSelectedProfile()
    {
        if (string.IsNullOrEmpty(SelectedProfileName)) return;

        _profileService.DeleteProfile(SelectedProfileName);
        RefreshProfileList();

        _isLoading = true;
        try
        {
            _selectedProfileName = null;
            OnPropertyChanged(nameof(SelectedProfileName));
            _currentProfileName = "";
            _originalProfileName = null;
            OnPropertyChanged(nameof(CurrentProfileName));
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void NewProfile()
    {
        _isLoading = true;
        try
        {
            Actions.Clear();
            _currentProfileName = "";
            _originalProfileName = null; // New profile, no original
            OnPropertyChanged(nameof(CurrentProfileName));
            _loopActions = true;
            OnPropertyChanged(nameof(LoopActions));
            _loopCount = 10;
            OnPropertyChanged(nameof(LoopCount));
            _delayBetweenLoops = 0;
            OnPropertyChanged(nameof(DelayBetweenLoops));
            SelectedAction = null;
            _selectedProfileName = null;
            OnPropertyChanged(nameof(SelectedProfileName));
            AddClickAction();
        }
        finally
        {
            _isLoading = false;
        }
    }

    #endregion

    public void Dispose()
    {
        _saveDebounceTimer.Stop();
        _hotkeyService.Dispose();
        _positionPicker.Dispose();
    }
}
