using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace AutoClickKey.Services;

public class AppSettings
{
    public string? LastProfileName { get; set; }

    public string? LastHotkey { get; set; }

    public string? LastWorkspaceName { get; set; }
}

public class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IFileSystem _fileSystem;

    [ExcludeFromCodeCoverage]
    public SettingsService() : this(new FileSystem())
    {
    }

    public SettingsService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey");

        _fileSystem.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");

        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    // Internal constructor for testing with custom path
    internal SettingsService(IFileSystem fileSystem, string settingsPath)
    {
        _fileSystem = fileSystem;
        _settingsPath = settingsPath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public AppSettings Load()
    {
        try
        {
            if (_fileSystem.FileExists(_settingsPath))
            {
                var json = _fileSystem.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            // Ignore errors, return default settings
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            _fileSystem.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
