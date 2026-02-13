using System;
using System.Threading;
using System.Threading.Tasks;
using AutoClickKey.Helpers;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class ClickerService
{
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event EventHandler<int>? ClickPerformed;

    public event EventHandler? Stopped;

    public bool IsRunning => _isRunning;

    public async Task StartAsync(ClickerSettings settings)
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        var clickCount = 0;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (settings.PositionMode == PositionMode.FixedPosition)
                {
                    Win32Api.MouseClickAt(
                        settings.FixedX,
                        settings.FixedY,
                        settings.MouseButton,
                        settings.ClickType == ClickType.Double);
                }
                else
                {
                    Win32Api.MouseClick(
                        settings.MouseButton,
                        settings.ClickType == ClickType.Double);
                }

                clickCount++;
                ClickPerformed?.Invoke(this, clickCount);

                if (settings.RepeatMode == RepeatMode.Count && clickCount >= settings.RepeatCount)
                {
                    break;
                }

                await Task.Delay(settings.IntervalMs, _cts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when stopped
        }
        finally
        {
            _isRunning = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }
}
