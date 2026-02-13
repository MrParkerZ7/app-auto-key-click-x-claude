using System.IO;
using System.Text.Json;
using AutoClickKey.Models;
using AutoClickKey.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoClickKey.Tests.Services;

public class ProfileServiceTests
{
    private const string TestProfilesDirectory = @"C:\TestProfiles";

    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _service = new ProfileService(_fileSystemMock.Object, TestProfilesDirectory);
    }

    [Fact]
    public void Constructor_CreatesProfilesDirectory()
    {
        // Assert
        _fileSystemMock.Verify(fs => fs.CreateDirectory(TestProfilesDirectory), Times.Once);
    }

    [Fact]
    public void SaveProfile_WritesJsonToFile()
    {
        // Arrange
        var profile = new Profile
        {
            Name = "TestProfile",
            LoopActions = true,
            LoopCount = 5
        };
        string? writtenContent = null;
        string? writtenPath = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, content) =>
            {
                writtenPath = path;
                writtenContent = content;
            });

        // Act
        _service.SaveProfile(profile);

        // Assert
        writtenPath.Should().Be(@"C:\TestProfiles\TestProfile.json");
        writtenContent.Should().NotBeNull();
        var deserializedProfile = JsonSerializer.Deserialize<Profile>(writtenContent!);
        deserializedProfile!.Name.Should().Be("TestProfile");
        deserializedProfile.LoopCount.Should().Be(5);
    }

    [Fact]
    public void SaveProfile_SanitizesFileName()
    {
        // Arrange
        var profile = new Profile { Name = "Test:Profile/Name" };
        string? writtenPath = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, _) => writtenPath = path);

        // Act
        _service.SaveProfile(profile);

        // Assert
        writtenPath.Should().Be(@"C:\TestProfiles\Test_Profile_Name.json");
    }

    [Fact]
    public void LoadProfile_ReturnsProfile_WhenFileExists()
    {
        // Arrange
        var profile = new Profile { Name = "TestProfile", LoopCount = 10 };
        var json = JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(json);

        // Act
        var result = _service.LoadProfile("TestProfile");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestProfile");
        result.LoopCount.Should().Be(10);
    }

    [Fact]
    public void LoadProfile_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = _service.LoadProfile("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllProfileNames_ReturnsEmptyList_WhenDirectoryDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.DirectoryExists(TestProfilesDirectory)).Returns(false);

        // Act
        var result = _service.GetAllProfileNames();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllProfileNames_ReturnsProfileNames_WhenFilesExist()
    {
        // Arrange
        var files = new[]
        {
            @"C:\TestProfiles\Profile1.json",
            @"C:\TestProfiles\Profile2.json",
            @"C:\TestProfiles\MyProfile.json"
        };

        _fileSystemMock.Setup(fs => fs.DirectoryExists(TestProfilesDirectory)).Returns(true);
        _fileSystemMock.Setup(fs => fs.GetFiles(TestProfilesDirectory, "*.json")).Returns(files);

        // Act
        var result = _service.GetAllProfileNames();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("Profile1");
        result.Should().Contain("Profile2");
        result.Should().Contain("MyProfile");
    }

    [Fact]
    public void DeleteProfile_DeletesFile_WhenFileExists()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);

        // Act
        _service.DeleteProfile("TestProfile");

        // Assert
        _fileSystemMock.Verify(fs => fs.DeleteFile(@"C:\TestProfiles\TestProfile.json"), Times.Once);
    }

    [Fact]
    public void DeleteProfile_DoesNothing_WhenFileDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        _service.DeleteProfile("NonExistent");

        // Assert
        _fileSystemMock.Verify(fs => fs.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ExportProfile_WritesToSpecifiedPath()
    {
        // Arrange
        var profile = new Profile { Name = "ExportTest" };
        var exportPath = @"D:\Exports\myprofile.json";
        string? writtenPath = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, _) => writtenPath = path);

        // Act
        _service.ExportProfile(profile, exportPath);

        // Assert
        writtenPath.Should().Be(exportPath);
    }

    [Fact]
    public void ImportProfile_ReturnsProfile_WhenFileExists()
    {
        // Arrange
        var profile = new Profile { Name = "ImportedProfile" };
        var json = JsonSerializer.Serialize(profile);
        var importPath = @"D:\Imports\profile.json";

        _fileSystemMock.Setup(fs => fs.FileExists(importPath)).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(importPath)).Returns(json);

        // Act
        var result = _service.ImportProfile(importPath);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("ImportedProfile");
    }

    [Fact]
    public void ImportProfile_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = _service.ImportProfile(@"D:\NonExistent.json");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFileSystem_CreatesDefaultDirectory()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystem>();
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey",
            "Profiles");

        // Act
        var service = new ProfileService(fileSystemMock.Object);

        // Assert
        fileSystemMock.Verify(fs => fs.CreateDirectory(expectedPath), Times.Once);
    }
}
