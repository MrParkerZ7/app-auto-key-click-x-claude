using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class WorkspaceRunnerService
{
    private readonly ActionRunnerService _actionRunner;
    private readonly ProfileService _profileService;
    private readonly ManualResetEventSlim _pauseEvent = new(true);

    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private bool _isPaused;

    public WorkspaceRunnerService(ActionRunnerService actionRunner, ProfileService profileService)
    {
        _actionRunner = actionRunner;
        _profileService = profileService;

        _actionRunner.ActionExecuted += (sender, e) => ActionExecuted?.Invoke(this, e);
    }

    public event EventHandler<WorkspaceProgressEventArgs>? ProgressChanged;

    public event EventHandler<(int ActionIndex, int LoopCount)>? ActionExecuted;

    public event EventHandler? Stopped;

    public event EventHandler? Paused;

    public event EventHandler? Resumed;

    public bool IsRunning => _isRunning;

    public bool IsPaused => _isPaused;

    public async Task RunAsync(Workspace workspace)
    {
        if (_isRunning || workspace.Jobs.Count == 0)
        {
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        var workspaceLoop = 0;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                workspaceLoop++;

                var enabledJobs = workspace.Jobs.Where(j => j.IsEnabled).ToList();

                for (var jobIndex = 0; jobIndex < enabledJobs.Count; jobIndex++)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    await WaitIfPausedAsync(_cts.Token);

                    var job = enabledJobs[jobIndex];
                    ReportProgress(job.Name, jobIndex, enabledJobs.Count, workspaceLoop, null);

                    for (var profileIndex = 0; profileIndex < job.ProfileNames.Count; profileIndex++)
                    {
                        if (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        await WaitIfPausedAsync(_cts.Token);

                        var profileName = job.ProfileNames[profileIndex];
                        var profile = _profileService.LoadProfile(profileName);

                        if (profile == null)
                        {
                            continue;
                        }

                        ReportProgress(job.Name, jobIndex, enabledJobs.Count, workspaceLoop, profileName);

                        var enabledActions = profile.Actions.Where(a => a.IsEnabled).ToList();
                        if (enabledActions.Count > 0)
                        {
                            await RunProfileActionsAsync(enabledActions, profile, _cts.Token);
                        }

                        if (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (profileIndex < job.ProfileNames.Count - 1 && job.DelayBetweenProfiles > 0)
                        {
                            await Task.Delay(job.DelayBetweenProfiles, _cts.Token);
                        }
                    }

                    if (_cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (jobIndex < enabledJobs.Count - 1 && workspace.DelayBetweenJobs > 0)
                    {
                        await Task.Delay(workspace.DelayBetweenJobs, _cts.Token);
                    }
                }

                if (!workspace.LoopWorkspace ||
                    (workspace.WorkspaceLoopCount > 0 && workspaceLoop >= workspace.WorkspaceLoopCount))
                {
                    break;
                }
            }
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            _isRunning = false;
            _isPaused = false;
            _pauseEvent.Set();
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Stop()
    {
        _isPaused = false;
        _pauseEvent.Set();
        _actionRunner.Stop();
        _cts?.Cancel();
    }

    public void Pause()
    {
        if (_isRunning && !_isPaused)
        {
            _isPaused = true;
            _pauseEvent.Reset();
            _actionRunner.Pause();
            Paused?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Resume()
    {
        if (_isRunning && _isPaused)
        {
            _isPaused = false;
            _pauseEvent.Set();
            _actionRunner.Resume();
            Resumed?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task RunProfileActionsAsync(IList<ActionItem> actions, Profile profile, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>();

        void OnStopped(object? sender, EventArgs e)
        {
            tcs.TrySetResult(true);
        }

        _actionRunner.Stopped += OnStopped;

        try
        {
            var runTask = _actionRunner.RunAsync(
                actions,
                profile.LoopActions,
                profile.LoopCount,
                profile.DelayBetweenLoops,
                profile.DelayBetweenActions,
                profile.RestoreMousePosition);

            await runTask;
            await tcs.Task;
        }
        finally
        {
            _actionRunner.Stopped -= OnStopped;
        }
    }

    private async Task WaitIfPausedAsync(CancellationToken ct)
    {
        while (_isPaused && !ct.IsCancellationRequested)
        {
            await Task.Run(() => _pauseEvent.Wait(ct), ct);
        }
    }

    private void ReportProgress(string jobName, int jobIndex, int totalJobs, int workspaceLoop, string? profileName)
    {
        ProgressChanged?.Invoke(this, new WorkspaceProgressEventArgs
        {
            CurrentJobName = jobName,
            CurrentJobIndex = jobIndex,
            TotalJobs = totalJobs,
            WorkspaceLoopCount = workspaceLoop,
            CurrentProfileName = profileName
        });
    }
}

public class WorkspaceProgressEventArgs : EventArgs
{
    public string CurrentJobName { get; set; } = string.Empty;

    public int CurrentJobIndex { get; set; }

    public int TotalJobs { get; set; }

    public int WorkspaceLoopCount { get; set; }

    public string? CurrentProfileName { get; set; }
}
