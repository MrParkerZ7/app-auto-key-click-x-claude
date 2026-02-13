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

## Testing

```bash
# Run tests with coverage (enforces 100% line coverage)
dotnet test

# Run tests without coverage threshold
dotnet test -p:CollectCoverage=false

# Run specific test class
dotnet test --filter "FullyQualifiedName~ActionItemTests"
```

**Test Stack**: xUnit, Moq, FluentAssertions

**Coverage**: 100% line coverage enforced via coverlet

## Code Quality

```bash
# Check formatting
dotnet format --verify-no-changes

# Fix formatting
dotnet format
```

**Tools**: StyleCop.Analyzers, EditorConfig

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

tests/AutoClickKey.Tests/
├── Models/             # ActionItem, Profile, Settings tests
├── Services/           # ProfileService, SettingsService tests
└── Helpers/            # RelayCommand, Converters tests
```

## Architecture

- **Framework**: .NET 8.0, WPF
- **Pattern**: MVVM (Model-View-ViewModel)
- **Windows API**: user32.dll via P/Invoke (SendInput, RegisterHotKey)
- **DI**: Constructor injection with IFileSystem abstraction for testability

## Code Style

- Use file-scoped namespaces (`namespace Foo;`)
- Use C# 12 features (primary constructors, collection expressions)
- Async methods suffixed with `Async`
- Services handle business logic, ViewModels handle UI state
- Use `SetProperty` in ViewModels for property change notification
- StyleCop enforces member ordering: constants, fields, constructors, events, properties, methods

## Key Bindings

- F6: Start/Stop automation (configurable)
- F8: Emergency stop (stops everything)

## Profiles

Profiles are saved as JSON in `%APPDATA%/AutoClickKey/Profiles/`
