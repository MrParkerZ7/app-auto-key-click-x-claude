using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class ProfileService
{
    private readonly string _profilesDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IFileSystem _fileSystem;

    public ProfileService() : this(new FileSystem())
    {
    }

    public ProfileService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _profilesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey",
            "Profiles");

        _fileSystem.CreateDirectory(_profilesDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    // Internal constructor for testing with custom directory
    internal ProfileService(IFileSystem fileSystem, string profilesDirectory)
    {
        _fileSystem = fileSystem;
        _profilesDirectory = profilesDirectory;
        _fileSystem.CreateDirectory(_profilesDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public void SaveProfile(Profile profile)
    {
        var fileName = SanitizeFileName(profile.Name) + ".json";
        var filePath = Path.Combine(_profilesDirectory, fileName);
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        _fileSystem.WriteAllText(filePath, json);
    }

    public Profile? LoadProfile(string name)
    {
        var fileName = SanitizeFileName(name) + ".json";
        var filePath = Path.Combine(_profilesDirectory, fileName);

        if (!_fileSystem.FileExists(filePath))
        {
            return null;
        }

        var json = _fileSystem.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Profile>(json, _jsonOptions);
    }

    public List<string> GetAllProfileNames()
    {
        var profiles = new List<string>();

        if (!_fileSystem.DirectoryExists(_profilesDirectory))
        {
            return profiles;
        }

        foreach (var file in _fileSystem.GetFiles(_profilesDirectory, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            profiles.Add(name);
        }

        return profiles;
    }

    public void DeleteProfile(string name)
    {
        var fileName = SanitizeFileName(name) + ".json";
        var filePath = Path.Combine(_profilesDirectory, fileName);

        if (_fileSystem.FileExists(filePath))
        {
            _fileSystem.DeleteFile(filePath);
        }
    }

    public void ExportProfile(Profile profile, string exportPath)
    {
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        _fileSystem.WriteAllText(exportPath, json);
    }

    public Profile? ImportProfile(string importPath)
    {
        if (!_fileSystem.FileExists(importPath))
        {
            return null;
        }

        var json = _fileSystem.ReadAllText(importPath);
        return JsonSerializer.Deserialize<Profile>(json, _jsonOptions);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
        {
            name = name.Replace(c, '_');
        }

        return name;
    }
}
