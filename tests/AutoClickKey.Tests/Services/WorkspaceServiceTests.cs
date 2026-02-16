using System;
using System.IO;
using System.Text.Json;
using AutoClickKey.Models;
using AutoClickKey.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoClickKey.Tests.Services;

public class WorkspaceServiceTests
{
    private const string TestWorkspacesDirectory = @"C:\TestWorkspaces";

    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly WorkspaceService _service;

    public WorkspaceServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _service = new WorkspaceService(_fileSystemMock.Object, TestWorkspacesDirectory);
    }

    [Fact]
    public void Constructor_CreatesWorkspacesDirectory()
    {
        _fileSystemMock.Verify(fs => fs.CreateDirectory(TestWorkspacesDirectory), Times.Once);
    }

    [Fact]
    public void SaveWorkspace_WritesJsonToFile()
    {
        var workspace = new Workspace
        {
            Name = "TestWorkspace",
            LoopWorkspace = true,
            WorkspaceLoopCount = 5
        };
        string? writtenContent = null;
        string? writtenPath = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, content) =>
            {
                writtenPath = path;
                writtenContent = content;
            });

        _service.SaveWorkspace(workspace);

        writtenPath.Should().Be(@"C:\TestWorkspaces\TestWorkspace.json");
        writtenContent.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<Workspace>(writtenContent!);
        deserialized!.Name.Should().Be("TestWorkspace");
        deserialized.WorkspaceLoopCount.Should().Be(5);
    }

    [Fact]
    public void SaveWorkspace_SanitizesFileName()
    {
        var workspace = new Workspace { Name = "Test:Workspace/Name" };
        string? writtenPath = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, _) => writtenPath = path);

        _service.SaveWorkspace(workspace);

        writtenPath.Should().Be(@"C:\TestWorkspaces\Test_Workspace_Name.json");
    }

    [Fact]
    public void LoadWorkspace_ReturnsWorkspace_WhenFileExists()
    {
        var workspace = new Workspace { Name = "TestWorkspace", WorkspaceLoopCount = 10 };
        var json = JsonSerializer.Serialize(workspace);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestWorkspaces\TestWorkspace.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestWorkspaces\TestWorkspace.json")).Returns(json);

        var result = _service.LoadWorkspace("TestWorkspace");

        result.Should().NotBeNull();
        result!.Name.Should().Be("TestWorkspace");
        result.WorkspaceLoopCount.Should().Be(10);
    }

    [Fact]
    public void LoadWorkspace_ReturnsNull_WhenFileDoesNotExist()
    {
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        var result = _service.LoadWorkspace("NonExistent");

        result.Should().BeNull();
    }

    [Fact]
    public void GetAllWorkspaceNames_ReturnsEmptyList_WhenDirectoryDoesNotExist()
    {
        _fileSystemMock.Setup(fs => fs.DirectoryExists(TestWorkspacesDirectory)).Returns(false);

        var result = _service.GetAllWorkspaceNames();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllWorkspaceNames_ReturnsWorkspaceNames_WhenFilesExist()
    {
        var files = new[]
        {
            @"C:\TestWorkspaces\Workspace1.json",
            @"C:\TestWorkspaces\Workspace2.json",
            @"C:\TestWorkspaces\MyWorkspace.json"
        };

        _fileSystemMock.Setup(fs => fs.DirectoryExists(TestWorkspacesDirectory)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetFiles(TestWorkspacesDirectory, "*.json")).Returns(files);

        var result = _service.GetAllWorkspaceNames();

        result.Should().HaveCount(3);
        result.Should().Contain("Workspace1");
        result.Should().Contain("Workspace2");
        result.Should().Contain("MyWorkspace");
    }

    [Fact]
    public void DeleteWorkspace_DeletesFile_WhenFileExists()
    {
        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestWorkspaces\TestWorkspace.json")).Returns(true);

        _service.DeleteWorkspace("TestWorkspace");

        _fileSystemMock.Verify(fs => fs.DeleteFile(@"C:\TestWorkspaces\TestWorkspace.json"), Times.Once);
    }

    [Fact]
    public void DeleteWorkspace_DoesNothing_WhenFileDoesNotExist()
    {
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        _service.DeleteWorkspace("NonExistent");

        _fileSystemMock.Verify(fs => fs.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Constructor_WithFileSystem_CreatesDefaultDirectory()
    {
        var fileSystemMock = new Mock<IFileSystem>();
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey",
            "Workspaces");

        var service = new WorkspaceService(fileSystemMock.Object);

        fileSystemMock.Verify(fs => fs.CreateDirectory(expectedPath), Times.Once);
    }

    [Fact]
    public void SaveWorkspace_PreservesJobsWithProfiles()
    {
        var job = new Job { Name = "Job1" };
        job.ProfileNames.Add("Profile1");
        job.ProfileNames.Add("Profile2");
        var workspace = new Workspace { Name = "TestWorkspace" };
        workspace.Jobs.Add(job);
        string? writtenContent = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((_, content) => writtenContent = content);

        _service.SaveWorkspace(workspace);

        var deserialized = JsonSerializer.Deserialize<Workspace>(writtenContent!);
        deserialized!.Jobs.Should().HaveCount(1);
        deserialized.Jobs[0].Name.Should().Be("Job1");
        deserialized.Jobs[0].ProfileNames.Should().Contain("Profile1");
        deserialized.Jobs[0].ProfileNames.Should().Contain("Profile2");
    }
}
