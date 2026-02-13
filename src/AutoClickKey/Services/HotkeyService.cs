using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using AutoClickKey.Helpers;

namespace AutoClickKey.Services;

public class HotkeyService : IDisposable
{
    private readonly Dictionary<int, Action> _hotkeyActions = new ();

    private IntPtr _windowHandle;
    private HwndSource? _source;
    private int _currentId;
    private bool _isDisposed;

    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(HwndHook);
    }

    public int RegisterHotkey(int modifiers, int key, Action callback)
    {
        _currentId++;

        if (!Win32Api.RegisterHotKey(_windowHandle, _currentId, modifiers, key))
        {
            throw new InvalidOperationException($"Failed to register hotkey. Error: {Marshal.GetLastWin32Error()}");
        }

        _hotkeyActions[_currentId] = callback;
        return _currentId;
    }

    public void UnregisterHotkey(int id)
    {
        Win32Api.UnregisterHotKey(_windowHandle, id);
        _hotkeyActions.Remove(id);
    }

    public void UnregisterAll()
    {
        foreach (var id in _hotkeyActions.Keys)
        {
            Win32Api.UnregisterHotKey(_windowHandle, id);
        }

        _hotkeyActions.Clear();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        UnregisterAll();
        _source?.RemoveHook(HwndHook);
        _isDisposed = true;
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Api.WMHOTKEY)
        {
            var id = wParam.ToInt32();
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                action.Invoke();
                handled = true;
            }
        }

        return IntPtr.Zero;
    }
}
