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

    public ProfileService()
    {
        _profilesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey",
            "Profiles");

        Directory.CreateDirectory(_profilesDirectory);

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
        File.WriteAllText(filePath, json);
    }

    public Profile? LoadProfile(string name)
    {
        var fileName = SanitizeFileName(name) + ".json";
        var filePath = Path.Combine(_profilesDirectory, fileName);

        if (!File.Exists(filePath))
            return null;

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Profile>(json, _jsonOptions);
    }

    public List<string> GetAllProfileNames()
    {
        var profiles = new List<string>();

        if (!Directory.Exists(_profilesDirectory))
            return profiles;

        foreach (var file in Directory.GetFiles(_profilesDirectory, "*.json"))
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

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public void ExportProfile(Profile profile, string exportPath)
    {
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        File.WriteAllText(exportPath, json);
    }

    public Profile? ImportProfile(string importPath)
    {
        if (!File.Exists(importPath))
            return null;

        var json = File.ReadAllText(importPath);
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
