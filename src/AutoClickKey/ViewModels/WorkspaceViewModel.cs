using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoClickKey.Helpers;
using AutoClickKey.Models;
using AutoClickKey.Services;

namespace AutoClickKey.ViewModels;

public class WorkspaceViewModel : ViewModelBase
{
    private readonly WorkspaceService _workspaceService;
    private readonly ProfileService _profileService;
    private readonly WorkspaceRunnerService _workspaceRunner;

    private string _currentWorkspaceName = string.Empty;
    private string? _selectedWorkspaceName;
    private Job? _selectedJob;
    private string? _selectedAvailableProfile;
    private string? _selectedJobProfile;
    private bool _loopWorkspace;
    private int _workspaceLoopCount = 1;
    private int _delayBetweenJobs;
    private bool _isRunning;
    private bool _isPaused;
    private string _statusText = string.Empty;

    public WorkspaceViewModel(WorkspaceService workspaceService, ProfileService profileService, WorkspaceRunnerService workspaceRunner)
    {
        _workspaceService = workspaceService;
        _profileService = profileService;
        _workspaceRunner = workspaceRunner;

        Jobs = new ObservableCollection<Job>();
        WorkspaceNames = new ObservableCollection<string>();
        AvailableProfiles = new ObservableCollection<string>();
        SelectedJobProfiles = new ObservableCollection<string>();

        _workspaceRunner.ProgressChanged += (_, e) =>
        {
            StatusText = e.CurrentProfileName != null
                ? $"Job: {e.CurrentJobName} ({e.CurrentJobIndex + 1}/{e.TotalJobs}) - Profile: {e.CurrentProfileName} - Loop: {e.WorkspaceLoopCount}"
                : $"Job: {e.CurrentJobName} ({e.CurrentJobIndex + 1}/{e.TotalJobs}) - Loop: {e.WorkspaceLoopCount}";
        };

        _workspaceRunner.Stopped += (_, _) =>
        {
            IsRunning = false;
            IsPaused = false;
            StatusText = "Workspace completed";
        };

        _workspaceRunner.Paused += (_, _) => IsPaused = true;
        _workspaceRunner.Resumed += (_, _) => IsPaused = false;

        AddJobCommand = new RelayCommand(AddJob);
        RemoveJobCommand = new RelayCommand(RemoveJob, () => SelectedJob != null);
        MoveJobUpCommand = new RelayCommand(MoveJobUp, () => SelectedJob != null && Jobs.IndexOf(SelectedJob) > 0);
        MoveJobDownCommand = new RelayCommand(MoveJobDown, () => SelectedJob != null && Jobs.IndexOf(SelectedJob) < Jobs.Count - 1);
        AddProfileToJobCommand = new RelayCommand(AddProfileToJob, () => SelectedJob != null && !string.IsNullOrEmpty(SelectedAvailableProfile));
        RemoveProfileFromJobCommand = new RelayCommand(RemoveProfileFromJob, () => SelectedJob != null && !string.IsNullOrEmpty(SelectedJobProfile));
        MoveProfileUpCommand = new RelayCommand(MoveProfileUp, () => SelectedJob != null && !string.IsNullOrEmpty(SelectedJobProfile) && SelectedJobProfiles.IndexOf(SelectedJobProfile!) > 0);
        MoveProfileDownCommand = new RelayCommand(MoveProfileDown, () => SelectedJob != null && !string.IsNullOrEmpty(SelectedJobProfile) && SelectedJobProfiles.IndexOf(SelectedJobProfile!) < SelectedJobProfiles.Count - 1);
        NewWorkspaceCommand = new RelayCommand(NewWorkspace);
        SaveWorkspaceCommand = new RelayCommand(SaveWorkspace, () => !string.IsNullOrWhiteSpace(CurrentWorkspaceName));
        DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace, () => !string.IsNullOrEmpty(SelectedWorkspaceName));
        RunWorkspaceCommand = new RelayCommand(RunWorkspace, () => Jobs.Count > 0 && !IsRunning);
        StopWorkspaceCommand = new RelayCommand(StopWorkspace, () => IsRunning);

        RefreshWorkspaceList();
        RefreshAvailableProfiles();
    }

    public ObservableCollection<Job> Jobs { get; }

    public ObservableCollection<string> WorkspaceNames { get; }

    public ObservableCollection<string> AvailableProfiles { get; }

    public ObservableCollection<string> SelectedJobProfiles { get; }

    public string CurrentWorkspaceName
    {
        get => _currentWorkspaceName;
        set
        {
            if (SetProperty(ref _currentWorkspaceName, value))
            {
                (SaveWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? SelectedWorkspaceName
    {
        get => _selectedWorkspaceName;
        set
        {
            if (SetProperty(ref _selectedWorkspaceName, value))
            {
                (DeleteWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    LoadWorkspace(value);
                }
            }
        }
    }

    public Job? SelectedJob
    {
        get => _selectedJob;
        set
        {
            if (SetProperty(ref _selectedJob, value))
            {
                RefreshSelectedJobProfiles();
                (RemoveJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveJobUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveJobDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AddProfileToJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RemoveProfileFromJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveProfileUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveProfileDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? SelectedAvailableProfile
    {
        get => _selectedAvailableProfile;
        set
        {
            if (SetProperty(ref _selectedAvailableProfile, value))
            {
                (AddProfileToJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? SelectedJobProfile
    {
        get => _selectedJobProfile;
        set
        {
            if (SetProperty(ref _selectedJobProfile, value))
            {
                (RemoveProfileFromJobCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveProfileUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveProfileDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool LoopWorkspace
    {
        get => _loopWorkspace;
        set => SetProperty(ref _loopWorkspace, value);
    }

    public int WorkspaceLoopCount
    {
        get => _workspaceLoopCount;
        set => SetProperty(ref _workspaceLoopCount, Math.Max(0, value));
    }

    public int DelayBetweenJobs
    {
        get => _delayBetweenJobs;
        set => SetProperty(ref _delayBetweenJobs, Math.Max(0, value));
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (SetProperty(ref _isRunning, value))
            {
                (RunWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (StopWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(RunButtonText));
            }
        }
    }

    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (SetProperty(ref _isPaused, value))
            {
                OnPropertyChanged(nameof(RunButtonText));
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string RunButtonText
    {
        get
        {
            if (IsRunning && IsPaused)
            {
                return "Resume";
            }

            return IsRunning ? "Pause" : "Run Workspace";
        }
    }

    public ICommand AddJobCommand { get; }

    public ICommand RemoveJobCommand { get; }

    public ICommand MoveJobUpCommand { get; }

    public ICommand MoveJobDownCommand { get; }

    public ICommand AddProfileToJobCommand { get; }

    public ICommand RemoveProfileFromJobCommand { get; }

    public ICommand MoveProfileUpCommand { get; }

    public ICommand MoveProfileDownCommand { get; }

    public ICommand NewWorkspaceCommand { get; }

    public ICommand SaveWorkspaceCommand { get; }

    public ICommand DeleteWorkspaceCommand { get; }

    public ICommand RunWorkspaceCommand { get; }

    public ICommand StopWorkspaceCommand { get; }

    public void RefreshAvailableProfiles()
    {
        AvailableProfiles.Clear();
        foreach (var name in _profileService.GetAllProfileNames())
        {
            AvailableProfiles.Add(name);
        }
    }

    public void Toggle()
    {
        if (IsRunning)
        {
            if (IsPaused)
            {
                _workspaceRunner.Resume();
            }
            else
            {
                _workspaceRunner.Pause();
            }
        }
        else
        {
            RunWorkspace();
        }
    }

    public void Stop()
    {
        _workspaceRunner.Stop();
    }

    private void AddJob()
    {
        var job = new Job { Name = $"Job {Jobs.Count + 1}" };
        Jobs.Add(job);
        SelectedJob = job;
        (RunWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void RemoveJob()
    {
        if (SelectedJob == null)
        {
            return;
        }

        var index = Jobs.IndexOf(SelectedJob);
        Jobs.Remove(SelectedJob);

        if (Jobs.Count > 0)
        {
            SelectedJob = Jobs[Math.Min(index, Jobs.Count - 1)];
        }
        else
        {
            SelectedJob = null;
        }

        (RunWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void MoveJobUp()
    {
        if (SelectedJob == null)
        {
            return;
        }

        var index = Jobs.IndexOf(SelectedJob);
        if (index > 0)
        {
            Jobs.Move(index, index - 1);
        }

        (MoveJobUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveJobDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void MoveJobDown()
    {
        if (SelectedJob == null)
        {
            return;
        }

        var index = Jobs.IndexOf(SelectedJob);
        if (index < Jobs.Count - 1)
        {
            Jobs.Move(index, index + 1);
        }

        (MoveJobUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveJobDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void AddProfileToJob()
    {
        if (SelectedJob == null || string.IsNullOrEmpty(SelectedAvailableProfile))
        {
            return;
        }

        SelectedJob.ProfileNames.Add(SelectedAvailableProfile);
        RefreshSelectedJobProfiles();
    }

    private void RemoveProfileFromJob()
    {
        if (SelectedJob == null || string.IsNullOrEmpty(SelectedJobProfile))
        {
            return;
        }

        var index = SelectedJobProfiles.IndexOf(SelectedJobProfile);
        SelectedJob.ProfileNames.Remove(SelectedJobProfile);
        RefreshSelectedJobProfiles();

        if (SelectedJobProfiles.Count > 0)
        {
            SelectedJobProfile = SelectedJobProfiles[Math.Min(index, SelectedJobProfiles.Count - 1)];
        }
    }

    private void MoveProfileUp()
    {
        if (SelectedJob == null || string.IsNullOrEmpty(SelectedJobProfile))
        {
            return;
        }

        var index = SelectedJob.ProfileNames.IndexOf(SelectedJobProfile);
        if (index > 0)
        {
            SelectedJob.ProfileNames.RemoveAt(index);
            SelectedJob.ProfileNames.Insert(index - 1, SelectedJobProfile);
            RefreshSelectedJobProfiles();
            SelectedJobProfile = SelectedJobProfiles[index - 1];
        }
    }

    private void MoveProfileDown()
    {
        if (SelectedJob == null || string.IsNullOrEmpty(SelectedJobProfile))
        {
            return;
        }

        var index = SelectedJob.ProfileNames.IndexOf(SelectedJobProfile);
        if (index < SelectedJob.ProfileNames.Count - 1)
        {
            SelectedJob.ProfileNames.RemoveAt(index);
            SelectedJob.ProfileNames.Insert(index + 1, SelectedJobProfile);
            RefreshSelectedJobProfiles();
            SelectedJobProfile = SelectedJobProfiles[index + 1];
        }
    }

    private void NewWorkspace()
    {
        Jobs.Clear();
        CurrentWorkspaceName = string.Empty;
        LoopWorkspace = false;
        WorkspaceLoopCount = 1;
        DelayBetweenJobs = 0;
        SelectedJob = null;
        _selectedWorkspaceName = null;
        OnPropertyChanged(nameof(SelectedWorkspaceName));
        (RunWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void SaveWorkspace()
    {
        if (string.IsNullOrWhiteSpace(CurrentWorkspaceName))
        {
            return;
        }

        var workspace = new Workspace
        {
            Name = CurrentWorkspaceName,
            Jobs = Jobs.ToList(),
            LoopWorkspace = LoopWorkspace,
            WorkspaceLoopCount = WorkspaceLoopCount,
            DelayBetweenJobs = DelayBetweenJobs,
            ModifiedAt = DateTime.Now
        };

        _workspaceService.SaveWorkspace(workspace);
        RefreshWorkspaceList();

        if (_selectedWorkspaceName != CurrentWorkspaceName)
        {
            _selectedWorkspaceName = CurrentWorkspaceName;
            OnPropertyChanged(nameof(SelectedWorkspaceName));
        }
    }

    private void LoadWorkspace(string name)
    {
        var workspace = _workspaceService.LoadWorkspace(name);
        if (workspace == null)
        {
            return;
        }

        Jobs.Clear();
        foreach (var job in workspace.Jobs)
        {
            Jobs.Add(job);
        }

        CurrentWorkspaceName = workspace.Name;
        LoopWorkspace = workspace.LoopWorkspace;
        WorkspaceLoopCount = workspace.WorkspaceLoopCount;
        DelayBetweenJobs = workspace.DelayBetweenJobs;

        if (Jobs.Count > 0)
        {
            SelectedJob = Jobs[0];
        }

        (RunWorkspaceCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void DeleteWorkspace()
    {
        if (string.IsNullOrEmpty(SelectedWorkspaceName))
        {
            return;
        }

        _workspaceService.DeleteWorkspace(SelectedWorkspaceName);
        RefreshWorkspaceList();

        _selectedWorkspaceName = null;
        OnPropertyChanged(nameof(SelectedWorkspaceName));
    }

    private void RunWorkspace()
    {
        if (Jobs.Count == 0 || IsRunning)
        {
            return;
        }

        var workspace = new Workspace
        {
            Name = CurrentWorkspaceName,
            Jobs = Jobs.ToList(),
            LoopWorkspace = LoopWorkspace,
            WorkspaceLoopCount = WorkspaceLoopCount,
            DelayBetweenJobs = DelayBetweenJobs
        };

        IsRunning = true;
        StatusText = "Starting workspace...";
        _ = _workspaceRunner.RunAsync(workspace);
    }

    private void StopWorkspace()
    {
        _workspaceRunner.Stop();
    }

    private void RefreshWorkspaceList()
    {
        var currentSelection = _selectedWorkspaceName;
        WorkspaceNames.Clear();
        foreach (var name in _workspaceService.GetAllWorkspaceNames())
        {
            WorkspaceNames.Add(name);
        }

        _selectedWorkspaceName = currentSelection;
        OnPropertyChanged(nameof(SelectedWorkspaceName));
    }

    private void RefreshSelectedJobProfiles()
    {
        SelectedJobProfiles.Clear();
        if (SelectedJob == null)
        {
            return;
        }

        foreach (var profileName in SelectedJob.ProfileNames)
        {
            SelectedJobProfiles.Add(profileName);
        }

        (MoveProfileUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (MoveProfileDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}
