# AutoClickKey

Windows automation tool for mouse clicking and keyboard input automation.

## Build & Run

```bash
# Build
dotnet build

# Run
dotnet run --project src/AutoClickKey

# Build release
dotnet build -c Release

# Publish single-file exe
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Project Structure

```
src/AutoClickKey/
├── Views/              # XAML views (currently MainWindow.xaml)
├── ViewModels/         # MVVM ViewModels
├── Models/             # Data models (settings, profiles)
├── Services/           # Core business logic
│   ├── ClickerService     # Mouse click automation
│   ├── KeyboardService    # Keyboard automation
│   ├── RecorderService    # Record/playback
│   ├── HotkeyService      # Global hotkeys
│   └── ProfileService     # Save/load profiles
└── Helpers/            # Utilities
    ├── Win32Api           # Windows API interop
    ├── RelayCommand       # MVVM commands
    └── Converters         # XAML converters
```

## Architecture

- **Framework**: .NET 8.0, WPF
- **Pattern**: MVVM (Model-View-ViewModel)
- **Windows API**: user32.dll via P/Invoke (SendInput, RegisterHotKey)

## Code Style

- Use file-scoped namespaces (`namespace Foo;`)
- Use C# 12 features (primary constructors, collection expressions)
- Async methods suffixed with `Async`
- Services handle business logic, ViewModels handle UI state
- Use `SetProperty` in ViewModels for property change notification

## Key Bindings

- F4: Start/Stop automation
- F8: Emergency stop (stops everything)

## Profiles

Profiles are saved as JSON in `%APPDATA%/AutoClickKey/Profiles/`
