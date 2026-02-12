using System;
using System.Threading;
using System.Threading.Tasks;
using AutoClickKey.Helpers;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class KeyboardService
{
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    // Virtual key codes for modifier keys
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_ALT = 0x12;
    private const ushort VK_SHIFT = 0x10;

    public bool IsRunning => _isRunning;
    public event EventHandler<int>? KeyPressed;
    public event EventHandler? Stopped;

    public async Task StartAsync(KeyboardSettings settings)
    {
        if (_isRunning) return;

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

    private async Task TypeTextAsync(string text, int intervalMs, CancellationToken ct)
    {
        foreach (var c in text)
        {
            if (ct.IsCancellationRequested) break;

            var keyCode = (ushort)char.ToUpper(c);

            // Handle shift for uppercase letters
            if (char.IsUpper(c))
            {
                Win32Api.KeyDown(VK_SHIFT);
            }

            Win32Api.KeyPress(keyCode);

            if (char.IsUpper(c))
            {
                Win32Api.KeyUp(VK_SHIFT);
            }

            await Task.Delay(intervalMs, ct);
        }
    }

    private void PressKeyWithModifiers(KeyboardSettings settings)
    {
        // Press modifiers
        if (settings.UseCtrl) Win32Api.KeyDown(VK_CONTROL);
        if (settings.UseAlt) Win32Api.KeyDown(VK_ALT);
        if (settings.UseShift) Win32Api.KeyDown(VK_SHIFT);

        // Press the key
        Win32Api.KeyPress(settings.KeyCode);

        // Release modifiers
        if (settings.UseShift) Win32Api.KeyUp(VK_SHIFT);
        if (settings.UseAlt) Win32Api.KeyUp(VK_ALT);
        if (settings.UseCtrl) Win32Api.KeyUp(VK_CONTROL);
    }

    public void Stop()
    {
        _cts?.Cancel();
    }
}
