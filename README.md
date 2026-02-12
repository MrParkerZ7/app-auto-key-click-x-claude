# AutoClickKey

A powerful Windows automation tool for mouse clicking and keyboard input automation, built with C# WPF.

## Features

### Auto Clicker
- **Click Types**: Left, Right, Middle mouse button
- **Click Modes**: Single click, Double click
- **Interval Control**: Custom delay between clicks (milliseconds)
- **Position Options**:
  - Click at current cursor position
  - Click at fixed X, Y coordinates
- **Repeat Options**: Set specific repeat count or infinite loop

### Auto Keyboard
- **Text Typing**: Automatically type text strings
- **Key Press**: Press single keys or key combinations (Ctrl+C, Alt+Tab, etc.)
- **Interval Control**: Custom delay between keystrokes (milliseconds)
- **Repeat Options**: Set specific repeat count or infinite loop

### Record & Playback
- **Mouse Recording**: Record mouse movements, clicks, and positions
- **Keyboard Recording**: Record keyboard inputs and timings
- **Playback**: Replay recorded actions with original or custom timing
- **Save/Export**: Save recordings for later use

### Profiles & Presets
- **Save Profiles**: Save current configuration as named profiles
- **Load Profiles**: Quickly switch between saved configurations
- **Import/Export**: Share profiles between devices

### Global Hotkeys
- **Start/Stop**: Default `F6` to toggle automation (customizable)
- **Emergency Stop**: Default `F8` to immediately stop all actions
- **Custom Bindings**: Assign your own hotkeys

### User Interface
- **Modern Design**: Clean WPF interface with dark/light theme
- **System Tray**: Minimize to system tray for background operation
- **Always on Top**: Optional window pinning
- **Real-time Status**: Live feedback on automation status and click/key count

## System Requirements

- **OS**: Windows 10 / 11
- **Runtime**: .NET 8.0 or later
- **Architecture**: x64

## Installation

### Option 1: Download Release
1. Go to [Releases](https://github.com/MrParkerZ7/app-auto-key-click-x-claude/releases)
2. Download the latest `.exe` or `.zip`
3. Run the application

### Option 2: Build from Source
```bash
git clone https://github.com/MrParkerZ7/app-auto-key-click-x-claude.git
cd app-auto-key-click-x-claude
dotnet build -c Release
```

## Usage

### Quick Start
1. Launch AutoClickKey
2. Configure your desired automation settings
3. Press `F6` to start/stop automation

### Auto Clicker Setup
1. Select click type (Left/Right/Middle)
2. Set interval in milliseconds
3. Choose position mode (Current cursor / Fixed position)
4. Set repeat count or enable infinite loop
5. Press `F6` to start

### Auto Keyboard Setup
1. Enter text to type or select keys to press
2. Set interval between keystrokes
3. Set repeat count or enable infinite loop
4. Press `F6` to start

### Recording Actions
1. Click "Record" button
2. Perform mouse/keyboard actions
3. Click "Stop Recording"
4. Use "Play" to replay actions

## Default Hotkeys

| Action | Hotkey |
|--------|--------|
| Start/Stop | `F6` |
| Emergency Stop | `F8` |
| Record | `Ctrl+R` |
| Play Recording | `Ctrl+P` |

## Project Structure

```
AutoClickKey/
├── src/AutoClickKey/              # Main WPF Application
│   ├── ViewModels/                # MVVM ViewModels
│   ├── Models/                    # Data Models
│   ├── Services/                  # Core Services
│   │   ├── ClickerService.cs      # Mouse automation
│   │   ├── KeyboardService.cs     # Keyboard automation
│   │   ├── RecorderService.cs     # Record & playback
│   │   ├── HotkeyService.cs       # Global hotkeys
│   │   └── ProfileService.cs      # Profile management
│   ├── Helpers/                   # Utility Classes
│   │   ├── Win32Api.cs            # Windows API Interop
│   │   ├── RelayCommand.cs        # MVVM commands
│   │   └── Converters.cs          # XAML converters
│   ├── MainWindow.xaml            # Main UI
│   └── App.xaml
├── README.md
├── CLAUDE.md                      # Development guide
└── AutoClickKey.sln
```

## Tech Stack

- **Framework**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Pattern**: MVVM (Model-View-ViewModel)
- **Windows API**: user32.dll (SendInput, SetWindowsHookEx)

## Roadmap

- [x] Project Setup
- [x] Core Auto Clicker functionality
- [x] Core Auto Keyboard functionality
- [x] Global Hotkey system
- [x] Record & Playback
- [x] Profile management
- [ ] Dark/Light theme
- [ ] System tray integration
- [ ] Settings persistence
- [ ] Installer/Release build

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This tool is intended for legitimate automation tasks, accessibility purposes, and personal productivity. Users are responsible for ensuring their use complies with applicable terms of service and laws.

---

**Made with C# + WPF**
