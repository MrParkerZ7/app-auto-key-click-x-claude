using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using AutoClickKey.Models;

namespace AutoClickKey.Services;

public class WorkspaceService
{
    private readonly string _workspacesDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IFileSystem _fileSystem;

    [ExcludeFromCodeCoverage]
    public WorkspaceService() : this(new FileSystem())
    {
    }

    public WorkspaceService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _workspacesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey",
            "Workspaces");

        _fileSystem.CreateDirectory(_workspacesDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    internal WorkspaceService(IFileSystem fileSystem, string workspacesDirectory)
    {
        _fileSystem = fileSystem;
        _workspacesDirectory = workspacesDirectory;
        _fileSystem.CreateDirectory(_workspacesDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public void SaveWorkspace(Workspace workspace)
    {
        var fileName = SanitizeFileName(workspace.Name) + ".json";
        var filePath = Path.Combine(_workspacesDirectory, fileName);
        var json = JsonSerializer.Serialize(workspace, _jsonOptions);
        _fileSystem.WriteAllText(filePath, json);
    }

    public Workspace? LoadWorkspace(string name)
    {
        var fileName = SanitizeFileName(name) + ".json";
        var filePath = Path.Combine(_workspacesDirectory, fileName);

        if (!_fileSystem.FileExists(filePath))
        {
            return null;
        }

        var json = _fileSystem.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Workspace>(json, _jsonOptions);
    }

    public List<string> GetAllWorkspaceNames()
    {
        var workspaces = new List<string>();

        if (!_fileSystem.DirectoryExists(_workspacesDirectory))
        {
            return workspaces;
        }

        foreach (var file in _fileSystem.GetFiles(_workspacesDirectory, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            workspaces.Add(name);
        }

        return workspaces;
    }

    public void DeleteWorkspace(string name)
    {
        var fileName = SanitizeFileName(name) + ".json";
        var filePath = Path.Combine(_workspacesDirectory, fileName);

        if (_fileSystem.FileExists(filePath))
        {
            _fileSystem.DeleteFile(filePath);
        }
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
