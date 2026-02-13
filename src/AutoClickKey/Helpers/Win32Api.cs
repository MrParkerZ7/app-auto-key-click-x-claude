using System;
using System.Runtime.InteropServices;

#pragma warning disable SA1201 // Elements should appear in the correct order - P/Invoke pattern

namespace AutoClickKey.Helpers;

/// <summary>
/// Windows API interop for mouse and keyboard simulation
/// </summary>
public static class Win32Api
{
    public const int MOUSEEVENTFLEFTDOWN = 0x0002;
    public const int MOUSEEVENTFLEFTUP = 0x0004;
    public const int MOUSEEVENTFRIGHTDOWN = 0x0008;
    public const int MOUSEEVENTFRIGHTUP = 0x0010;
    public const int MOUSEEVENTFMIDDLEDOWN = 0x0020;
    public const int MOUSEEVENTFMIDDLEUP = 0x0040;
    public const int MOUSEEVENTFABSOLUTE = 0x8000;
    public const int MOUSEEVENTFMOVE = 0x0001;
    public const int KEYEVENTFKEYDOWN = 0x0000;
    public const int KEYEVENTFKEYUP = 0x0002;
    public const int KEYEVENTFEXTENDEDKEY = 0x0001;
    public const int INPUTMOUSE = 0;
    public const int INPUTKEYBOARD = 1;
    public const int WMHOTKEY = 0x0312;
    public const int MODNONE = 0x0000;
    public const int MODALT = 0x0001;
    public const int MODCONTROL = 0x0002;
    public const int MODSHIFT = 0x0004;
    public const int MODWIN = 0x0008;
    public const int WHMOUSELL = 14;
    public const int WMLBUTTONDOWN = 0x0201;
    public const int WMRBUTTONDOWN = 0x0204;
    public const int WMMBUTTONDOWN = 0x0207;

    public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT Pt;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public int Type;
        public INPUTUNION Union;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUTUNION
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mi;
        [FieldOffset(0)]
        public KEYBDINPUT Ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public int MouseData;
        public int DwFlags;
        public int Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    public static POINT GetCursorPosition()
    {
        GetCursorPos(out POINT point);
        return point;
    }

    public static void MouseClick(MouseButton button, bool doubleClick = false)
    {
        int downFlag, upFlag;

        switch (button)
        {
            case MouseButton.Left:
                downFlag = MOUSEEVENTFLEFTDOWN;
                upFlag = MOUSEEVENTFLEFTUP;
                break;
            case MouseButton.Right:
                downFlag = MOUSEEVENTFRIGHTDOWN;
                upFlag = MOUSEEVENTFRIGHTUP;
                break;
            case MouseButton.Middle:
                downFlag = MOUSEEVENTFMIDDLEDOWN;
                upFlag = MOUSEEVENTFMIDDLEUP;
                break;
            default:
                return;
        }

        var inputs = new INPUT[2];

        inputs[0] = new INPUT
        {
            Type = INPUTMOUSE,
            Union = new INPUTUNION
            {
                Mi = new MOUSEINPUT { DwFlags = downFlag }
            }
        };

        inputs[1] = new INPUT
        {
            Type = INPUTMOUSE,
            Union = new INPUTUNION
            {
                Mi = new MOUSEINPUT { DwFlags = upFlag }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());

        if (doubleClick)
        {
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        }
    }

    public static void MouseClickAt(int x, int y, MouseButton button, bool doubleClick = false)
    {
        SetCursorPos(x, y);
        MouseClick(button, doubleClick);
    }

    public static void KeyPress(ushort keyCode)
    {
        var inputs = new INPUT[2];

        inputs[0] = new INPUT
        {
            Type = INPUTKEYBOARD,
            Union = new INPUTUNION
            {
                Ki = new KEYBDINPUT
                {
                    WVk = keyCode,
                    DwFlags = KEYEVENTFKEYDOWN
                }
            }
        };

        inputs[1] = new INPUT
        {
            Type = INPUTKEYBOARD,
            Union = new INPUTUNION
            {
                Ki = new KEYBDINPUT
                {
                    WVk = keyCode,
                    DwFlags = KEYEVENTFKEYUP
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    public static void KeyDown(ushort keyCode)
    {
        var inputs = new INPUT[1];

        inputs[0] = new INPUT
        {
            Type = INPUTKEYBOARD,
            Union = new INPUTUNION
            {
                Ki = new KEYBDINPUT
                {
                    WVk = keyCode,
                    DwFlags = KEYEVENTFKEYDOWN
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    public static void KeyUp(ushort keyCode)
    {
        var inputs = new INPUT[1];

        inputs[0] = new INPUT
        {
            Type = INPUTKEYBOARD,
            Union = new INPUTUNION
            {
                Ki = new KEYBDINPUT
                {
                    WVk = keyCode,
                    DwFlags = KEYEVENTFKEYUP
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }
}

public enum MouseButton
{
    Left,
    Right,
    Middle
}
