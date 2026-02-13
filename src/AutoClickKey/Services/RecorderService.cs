using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoClickKey.Helpers;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class RecorderService
{
    private readonly List<RecordedAction> _recordedActions = new ();

    private CancellationTokenSource? _recordCts;
    private CancellationTokenSource? _playCts;
    private Stopwatch? _stopwatch;
    private long _lastTimestamp;
    private bool _isRecording;
    private bool _isPlaying;

    public event EventHandler<RecordedAction>? ActionRecorded;

    public event EventHandler? RecordingStopped;

    public event EventHandler? PlaybackStopped;

    public bool IsRecording => _isRecording;

    public bool IsPlaying => _isPlaying;

    public IReadOnlyList<RecordedAction> RecordedActions => _recordedActions.AsReadOnly();

    public void StartRecording()
    {
        if (_isRecording)
        {
            return;
        }

        _recordedActions.Clear();
        _isRecording = true;
        _stopwatch = Stopwatch.StartNew();
        _lastTimestamp = 0;
        _recordCts = new CancellationTokenSource();

        // Start polling for mouse/keyboard state
        Task.Run(() => RecordingLoop(_recordCts.Token));
    }

    public void StopRecording()
    {
        if (!_isRecording)
        {
            return;
        }

        _recordCts?.Cancel();
        _stopwatch?.Stop();
        _isRecording = false;
        RecordingStopped?.Invoke(this, EventArgs.Empty);
    }

    public async Task PlayAsync(float speedMultiplier = 1.0f)
    {
        if (_isPlaying || _recordedActions.Count == 0)
        {
            return;
        }

        _isPlaying = true;
        _playCts = new CancellationTokenSource();

        try
        {
            foreach (var action in _recordedActions)
            {
                if (_playCts.Token.IsCancellationRequested)
                {
                    break;
                }

                var delay = (int)(action.DelayFromPrevious / speedMultiplier);
                if (delay > 0)
                {
                    await Task.Delay(delay, _playCts.Token);
                }

                ExecuteAction(action);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when stopped
        }
        finally
        {
            _isPlaying = false;
            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public void StopPlayback()
    {
        _playCts?.Cancel();
    }

    public void ClearRecording()
    {
        _recordedActions.Clear();
    }

    public void LoadActions(List<RecordedAction> actions)
    {
        _recordedActions.Clear();
        _recordedActions.AddRange(actions);
    }

    private async Task RecordingLoop(CancellationToken ct)
    {
        var lastPosition = Win32Api.GetCursorPosition();
        var lastLeftState = false;
        var lastRightState = false;
        var lastMiddleState = false;

        while (!ct.IsCancellationRequested)
        {
            var currentPosition = Win32Api.GetCursorPosition();
            var timestamp = _stopwatch!.ElapsedMilliseconds;

            // Check for mouse movement
            if (currentPosition.X != lastPosition.X || currentPosition.Y != lastPosition.Y)
            {
                RecordAction(new RecordedAction
                {
                    Type = ActionType.MouseMove,
                    X = currentPosition.X,
                    Y = currentPosition.Y,
                    Timestamp = timestamp,
                    DelayFromPrevious = (int)(timestamp - _lastTimestamp)
                });
                lastPosition = currentPosition;
            }

            // Check for left click
            var leftState = (Win32Api.GetAsyncKeyState(0x01) & 0x8000) != 0;
            if (leftState && !lastLeftState)
            {
                RecordAction(new RecordedAction
                {
                    Type = ActionType.MouseClick,
                    X = currentPosition.X,
                    Y = currentPosition.Y,
                    MouseButton = MouseButton.Left,
                    Timestamp = timestamp,
                    DelayFromPrevious = (int)(timestamp - _lastTimestamp)
                });
            }

            lastLeftState = leftState;

            // Check for right click
            var rightState = (Win32Api.GetAsyncKeyState(0x02) & 0x8000) != 0;
            if (rightState && !lastRightState)
            {
                RecordAction(new RecordedAction
                {
                    Type = ActionType.MouseClick,
                    X = currentPosition.X,
                    Y = currentPosition.Y,
                    MouseButton = MouseButton.Right,
                    Timestamp = timestamp,
                    DelayFromPrevious = (int)(timestamp - _lastTimestamp)
                });
            }

            lastRightState = rightState;

            // Check for middle click
            var middleState = (Win32Api.GetAsyncKeyState(0x04) & 0x8000) != 0;
            if (middleState && !lastMiddleState)
            {
                RecordAction(new RecordedAction
                {
                    Type = ActionType.MouseClick,
                    X = currentPosition.X,
                    Y = currentPosition.Y,
                    MouseButton = MouseButton.Middle,
                    Timestamp = timestamp,
                    DelayFromPrevious = (int)(timestamp - _lastTimestamp)
                });
            }

            lastMiddleState = middleState;

            await Task.Delay(10, ct).ConfigureAwait(false);
        }
    }

    private void RecordAction(RecordedAction action)
    {
        _recordedActions.Add(action);
        _lastTimestamp = action.Timestamp;
        ActionRecorded?.Invoke(this, action);
    }

    private void ExecuteAction(RecordedAction action)
    {
        switch (action.Type)
        {
            case ActionType.MouseMove:
                Win32Api.SetCursorPos(action.X, action.Y);
                break;

            case ActionType.MouseClick:
                Win32Api.MouseClickAt(action.X, action.Y, action.MouseButton);
                break;

            case ActionType.KeyPress:
                if (action.IsKeyDown)
                {
                    Win32Api.KeyDown(action.KeyCode);
                }
                else
                {
                    Win32Api.KeyUp(action.KeyCode);
                }

                break;
        }
    }
}
