using System.IO;
using System.Text.Json;
using AutoClickKey.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoClickKey.Tests.Services;

public class SettingsServiceTests
{
    private const string TestSettingsPath = @"C:\TestSettings\settings.json";

    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _service = new SettingsService(_fileSystemMock.Object, TestSettingsPath);
    }

    [Fact]
    public void Load_ReturnsDefaultSettings_WhenFileDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(TestSettingsPath)).Returns(false);

        // Act
        var result = _service.Load();

        // Assert
        result.Should().NotBeNull();
        result.LastProfileName.Should().BeNull();
        result.LastHotkey.Should().BeNull();
    }

    [Fact]
    public void Load_ReturnsSettings_WhenFileExists()
    {
        // Arrange
        var settings = new AppSettings
        {
            LastProfileName = "MyProfile",
            LastHotkey = "F5"
        };
        var json = JsonSerializer.Serialize(settings);

        _fileSystemMock.Setup(fs => fs.FileExists(TestSettingsPath)).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(TestSettingsPath)).Returns(json);

        // Act
        var result = _service.Load();

        // Assert
        result.LastProfileName.Should().Be("MyProfile");
        result.LastHotkey.Should().Be("F5");
    }

    [Fact]
    public void Load_ReturnsDefaultSettings_WhenReadThrows()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(TestSettingsPath)).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(TestSettingsPath)).Throws<IOException>();

        // Act
        var result = _service.Load();

        // Assert
        result.Should().NotBeNull();
        result.LastProfileName.Should().BeNull();
    }

    [Fact]
    public void Load_ReturnsDefaultSettings_WhenJsonIsInvalid()
    {
        // Arrange
        _fileSystemMock.Setup(fs => fs.FileExists(TestSettingsPath)).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(TestSettingsPath)).Returns("invalid json {{{");

        // Act
        var result = _service.Load();

        // Assert
        result.Should().NotBeNull();
        result.LastProfileName.Should().BeNull();
    }

    [Fact]
    public void Save_WritesSettingsToFile()
    {
        // Arrange
        var settings = new AppSettings
        {
            LastProfileName = "SavedProfile",
            LastHotkey = "F6"
        };
        string? writtenContent = null;

        _fileSystemMock.Setup(fs => fs.WriteAllText(TestSettingsPath, It.IsAny<string>()))
            .Callback<string, string>((_, content) => writtenContent = content);

        // Act
        _service.Save(settings);

        // Assert
        writtenContent.Should().NotBeNull();
        var deserializedSettings = JsonSerializer.Deserialize<AppSettings>(writtenContent!);
        deserializedSettings!.LastProfileName.Should().Be("SavedProfile");
        deserializedSettings.LastHotkey.Should().Be("F6");
    }

    [Fact]
    public void Save_DoesNotThrow_WhenWriteFails()
    {
        // Arrange
        var settings = new AppSettings { LastProfileName = "Test" };
        _fileSystemMock.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<IOException>();

        // Act
        Action act = () => _service.Save(settings);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithFileSystem_CreatesDefaultDirectory()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystem>();
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickKey");

        // Act
        var service = new SettingsService(fileSystemMock.Object);

        // Assert
        fileSystemMock.Verify(fs => fs.CreateDirectory(expectedPath), Times.Once);
    }
}
