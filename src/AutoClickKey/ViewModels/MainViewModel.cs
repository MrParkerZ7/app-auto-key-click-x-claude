using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
    private Window? _mainWindow;

    public MainViewModel()
    {
        _actionRunner = new ActionRunnerService();
        _profileService = new ProfileService();
        _hotkeyService = new HotkeyService();
        _positionPicker = new PositionPickerService();

        // Setup position picker events
        _positionPicker.PositionPicked += (_, pos) =>
            Application.Current.Dispatcher.Invoke(() => OnPositionPicked(pos.X, pos.Y));
        _positionPicker.PickingCancelled += (_, _) =>
            Application.Current.Dispatcher.Invoke(() => OnPickingCancelled());

        Actions = new ObservableCollection<ActionItem>();
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
        StopCommand = new RelayCommand(Stop);
        AddClickCommand = new RelayCommand(AddClickAction);
        AddKeyCommand = new RelayCommand(AddKeyAction);
        AddDelayCommand = new RelayCommand(AddDelayAction);
        RemoveActionCommand = new RelayCommand(RemoveSelectedAction, () => SelectedAction != null);
        MoveUpCommand = new RelayCommand(MoveActionUp, () => SelectedAction != null && Actions.IndexOf(SelectedAction) > 0);
        MoveDownCommand = new RelayCommand(MoveActionDown, () => SelectedAction != null && Actions.IndexOf(SelectedAction) < Actions.Count - 1);
        DuplicateCommand = new RelayCommand(DuplicateAction, () => SelectedAction != null);
        ClearAllCommand = new RelayCommand(ClearAllActions, () => Actions.Count > 0);
        SaveProfileCommand = new RelayCommand(SaveProfile);
        LoadProfileCommand = new RelayCommand(LoadSelectedProfile, () => !string.IsNullOrEmpty(SelectedProfileName));
        DeleteProfileCommand = new RelayCommand(DeleteSelectedProfile, () => !string.IsNullOrEmpty(SelectedProfileName));
        NewProfileCommand = new RelayCommand(NewProfile);
        PickPositionCommand = new RelayCommand(PickPosition);

        // Load profile names
        RefreshProfileList();

        // Add default action
        if (Actions.Count == 0)
        {
            AddClickAction();
        }
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

    private string _currentProfileName = "Untitled";
    public string CurrentProfileName
    {
        get => _currentProfileName;
        set => SetProperty(ref _currentProfileName, value);
    }

    private string? _selectedProfileName;
    public string? SelectedProfileName
    {
        get => _selectedProfileName;
        set
        {
            if (SetProperty(ref _selectedProfileName, value))
            {
                (LoadProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    private bool _loopActions = true;
    public bool LoopActions
    {
        get => _loopActions;
        set => SetProperty(ref _loopActions, value);
    }

    private int _loopCount;
    public int LoopCount
    {
        get => _loopCount;
        set => SetProperty(ref _loopCount, Math.Max(0, value));
    }

    private int _delayBetweenLoops;
    public int DelayBetweenLoops
    {
        get => _delayBetweenLoops;
        set => SetProperty(ref _delayBetweenLoops, Math.Max(0, value));
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
            }
        }
    }

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

    #endregion

    #region Commands

    public ICommand ToggleCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand AddClickCommand { get; }
    public ICommand AddKeyCommand { get; }
    public ICommand AddDelayCommand { get; }
    public ICommand RemoveActionCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand DuplicateCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand LoadProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand NewProfileCommand { get; }
    public ICommand PickPositionCommand { get; }

    #endregion

    #region Hotkey Setup

    public void InitializeHotkeys(Window window)
    {
        _mainWindow = window;
        _hotkeyService.Initialize(window);

        // F4 - Start/Stop
        _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, 0x73, Toggle);

        // F8 - Emergency Stop
        _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, 0x77, Stop);

        // Escape - Cancel picking
        _hotkeyService.RegisterHotkey(Win32Api.MOD_NONE, 0x1B, CancelPicking);
    }

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
            UseCurrentPosition = true,
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
            StatusText = $"Running... Loop {CurrentLoopCount} - Press F4 to Stop";
        }
        else
        {
            StatusText = "Ready - Press F4 to Start";
        }
    }

    #endregion

    #region Profile Methods

    private void RefreshProfileList()
    {
        ProfileNames.Clear();
        foreach (var name in _profileService.GetAllProfileNames())
        {
            ProfileNames.Add(name);
        }
    }

    private void SaveProfile()
    {
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
        RefreshProfileList();
        SelectedProfileName = CurrentProfileName;
    }

    private void LoadSelectedProfile()
    {
        if (string.IsNullOrEmpty(SelectedProfileName)) return;

        var profile = _profileService.LoadProfile(SelectedProfileName);
        if (profile == null) return;

        Actions.Clear();
        foreach (var action in profile.Actions)
        {
            Actions.Add(action);
        }

        CurrentProfileName = profile.Name;
        LoopActions = profile.LoopActions;
        LoopCount = profile.LoopCount;
        DelayBetweenLoops = profile.DelayBetweenLoops;

        if (Actions.Count > 0)
        {
            SelectedAction = Actions[0];
        }
    }

    private void DeleteSelectedProfile()
    {
        if (string.IsNullOrEmpty(SelectedProfileName)) return;

        _profileService.DeleteProfile(SelectedProfileName);
        RefreshProfileList();
        SelectedProfileName = null;
    }

    private void NewProfile()
    {
        Actions.Clear();
        CurrentProfileName = "Untitled";
        LoopActions = true;
        LoopCount = 0;
        DelayBetweenLoops = 0;
        SelectedAction = null;
        AddClickAction();
    }

    #endregion

    public void Dispose()
    {
        _hotkeyService.Dispose();
        _positionPicker.Dispose();
    }
}
