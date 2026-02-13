using System;
using System.Threading;
using System.Threading.Tasks;
using AutoClickKey.Helpers;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class KeyboardService
{
    // Virtual key codes for modifier keys
    private const ushort VKCONTROL = 0x11;
    private const ushort VKALT = 0x12;
    private const ushort VKSHIFT = 0x10;

    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event EventHandler<int>? KeyPressed;

    public event EventHandler? Stopped;

    public bool IsRunning => _isRunning;

    public async Task StartAsync(KeyboardSettings settings)
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        var keyCount = 0;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (settings.Mode == KeyboardMode.TypeText)
                {
                    await TypeTextAsync(settings.TextToType, settings.IntervalMs, _cts.Token);
                    keyCount += settings.TextToType.Length;
                }
                else
                {
                    PressKeyWithModifiers(settings);
                    keyCount++;
                }

                KeyPressed?.Invoke(this, keyCount);

                if (settings.RepeatMode == RepeatMode.Count && keyCount >= settings.RepeatCount)
                {
                    break;
                }

                if (settings.Mode == KeyboardMode.PressKey)
                {
                    await Task.Delay(settings.IntervalMs, _cts.Token);
                }
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

    private async Task TypeTextAsync(string text, int intervalMs, CancellationToken ct)
    {
        foreach (var c in text)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var keyCode = (ushort)char.ToUpper(c);

            // Handle shift for uppercase letters
            if (char.IsUpper(c))
            {
                Win32Api.KeyDown(VKSHIFT);
            }

            Win32Api.KeyPress(keyCode);

            if (char.IsUpper(c))
            {
                Win32Api.KeyUp(VKSHIFT);
            }

            await Task.Delay(intervalMs, ct);
        }
    }

    private void PressKeyWithModifiers(KeyboardSettings settings)
    {
        // Press modifiers
        if (settings.UseCtrl)
        {
            Win32Api.KeyDown(VKCONTROL);
        }

        if (settings.UseAlt)
        {
            Win32Api.KeyDown(VKALT);
        }

        if (settings.UseShift)
        {
            Win32Api.KeyDown(VKSHIFT);
        }

        // Press the key
        Win32Api.KeyPress(settings.KeyCode);

        // Release modifiers
        if (settings.UseShift)
        {
            Win32Api.KeyUp(VKSHIFT);
        }

        if (settings.UseAlt)
        {
            Win32Api.KeyUp(VKALT);
        }

        if (settings.UseCtrl)
        {
            Win32Api.KeyUp(VKCONTROL);
        }
    }
}
