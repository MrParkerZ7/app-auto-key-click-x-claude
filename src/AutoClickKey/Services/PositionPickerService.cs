using System;
using System.Runtime.InteropServices;
using AutoClickKey.Helpers;

namespace AutoClickKey.Services;

public class PositionPickerService : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private Win32Api.LowLevelMouseProc? _proc;
    private bool _isPicking;
    private bool _isDisposed;

    public event EventHandler<(int X, int Y)>? PositionPicked;

    public event EventHandler? PickingCancelled;

    public bool IsPicking => _isPicking;

    public void StartPicking()
    {
        if (_isPicking)
        {
            return;
        }

        _isPicking = true;
        _proc = HookCallback;
        _hookId = SetHook(_proc);
    }

    public void CancelPicking()
    {
        if (!_isPicking)
        {
            return;
        }

        StopHook();
        _isPicking = false;
        PickingCancelled?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        StopHook();
        _isDisposed = true;
    }

    private IntPtr SetHook(Win32Api.LowLevelMouseProc proc)
    {
        var moduleHandle = Win32Api.GetModuleHandle(null!);
        return Win32Api.SetWindowsHookEx(Win32Api.WHMOUSELL, proc, moduleHandle, 0);
    }

    private void StopHook()
    {
        if (_hookId != IntPtr.Zero)
        {
            Win32Api.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        _proc = null;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _isPicking)
        {
            var message = wParam.ToInt32();

            // Check for any mouse button down
            if (message == Win32Api.WMLBUTTONDOWN ||
                message == Win32Api.WMRBUTTONDOWN ||
                message == Win32Api.WMMBUTTONDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<Win32Api.MSLLHOOKSTRUCT>(lParam);
                var x = hookStruct.Pt.X;
                var y = hookStruct.Pt.Y;

                // Stop picking
                StopHook();
                _isPicking = false;

                // Raise event on UI thread
                PositionPicked?.Invoke(this, (x, y));

                // Block this click from going through
                return (IntPtr)1;
            }
        }

        return Win32Api.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }
}
