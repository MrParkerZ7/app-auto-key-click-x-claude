using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoClickKey.Helpers;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class ActionRunnerService
{
    private const ushort VKCONTROL = 0x11;
    private const ushort VKALT = 0x12;
    private const ushort VKSHIFT = 0x10;

    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event EventHandler<(int ActionIndex, int LoopCount)>? ActionExecuted;

    public event EventHandler? Stopped;

    public bool IsRunning => _isRunning;

    public async Task RunAsync(IList<ActionItem> actions, bool loop, int loopCount, int delayBetweenLoops, int delayBetweenActions = 0, bool restoreMousePosition = false)
    {
        if (_isRunning || actions.Count == 0)
        {
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        var currentLoop = 0;

        // Save mouse position before starting
        Win32Api.POINT? savedPosition = null;
        if (restoreMousePosition)
        {
            savedPosition = Win32Api.GetCursorPosition();
        }

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                currentLoop++;

                for (var i = 0; i < actions.Count; i++)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    var action = actions[i];
                    if (!action.IsEnabled)
                    {
                        continue;
                    }

                    for (var r = 0; r < action.RepeatCount; r++)
                    {
                        if (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        await ExecuteActionAsync(action, _cts.Token);
                        ActionExecuted?.Invoke(this, (i, currentLoop));

                        if (r < action.RepeatCount - 1)
                        {
                            await Task.Delay(action.DelayMs, _cts.Token);
                        }
                    }

                    // Delay between actions (global setting)
                    if (i < actions.Count - 1 && delayBetweenActions > 0)
                    {
                        await Task.Delay(delayBetweenActions, _cts.Token);
                    }
                }

                if (!loop || (loopCount > 0 && currentLoop >= loopCount))
                {
                    break;
                }

                if (delayBetweenLoops > 0)
                {
                    await Task.Delay(delayBetweenLoops, _cts.Token);
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when stopped
        }
        finally
        {
            // Restore mouse position if requested
            if (savedPosition.HasValue)
            {
                Win32Api.SetCursorPos(savedPosition.Value.X, savedPosition.Value.Y);
            }

            _isRunning = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private static ushort ParseKeyCode(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return 0;
        }

        if (key.Length == 1)
        {
            var c = char.ToUpper(key[0]);
            if (c >= '0' && c <= '9')
            {
                return (ushort)(0x30 + (c - '0'));
            }

            if (c >= 'A' && c <= 'Z')
            {
                return (ushort)(0x41 + (c - 'A'));
            }

            return (ushort)c;
        }

        return key.ToUpper() switch
        {
            "ENTER" => 0x0D,
            "TAB" => 0x09,
            "SPACE" => 0x20,
            "BACKSPACE" => 0x08,
            "DELETE" => 0x2E,
            "ESCAPE" or "ESC" => 0x1B,
            "UP" => 0x26,
            "DOWN" => 0x28,
            "LEFT" => 0x25,
            "RIGHT" => 0x27,
            "HOME" => 0x24,
            "END" => 0x23,
            "PAGEUP" => 0x21,
            "PAGEDOWN" => 0x22,
            "INSERT" => 0x2D,
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

    private async Task ExecuteActionAsync(ActionItem action, CancellationToken ct)
    {
        switch (action.Type)
        {
            case ActionItemType.Click:
                ExecuteClick(action);
                break;

            case ActionItemType.KeyPress:
                ExecuteKeyPress(action);
                break;

            case ActionItemType.Delay:
                await Task.Delay(action.DelayMs, ct);
                break;
        }
    }

    private void ExecuteClick(ActionItem action)
    {
        var mouseButton = (MouseButton)action.MouseButton;
        var isDouble = action.ClickType == 1;

        if (action.UseCurrentPosition)
        {
            Win32Api.MouseClick(mouseButton, isDouble);
        }
        else
        {
            Win32Api.MouseClickAt(action.FixedX, action.FixedY, mouseButton, isDouble);
        }
    }

    private void ExecuteKeyPress(ActionItem action)
    {
        var keyCode = ParseKeyCode(action.Key);
        if (keyCode == 0)
        {
            return;
        }

        // Press modifiers
        if (action.UseCtrl)
        {
            Win32Api.KeyDown(VKCONTROL);
        }

        if (action.UseAlt)
        {
            Win32Api.KeyDown(VKALT);
        }

        if (action.UseShift)
        {
            Win32Api.KeyDown(VKSHIFT);
        }

        // Press the key
        Win32Api.KeyPress(keyCode);

        // Release modifiers
        if (action.UseShift)
        {
            Win32Api.KeyUp(VKSHIFT);
        }

        if (action.UseAlt)
        {
            Win32Api.KeyUp(VKALT);
        }

        if (action.UseCtrl)
        {
            Win32Api.KeyUp(VKCONTROL);
        }
    }
}
