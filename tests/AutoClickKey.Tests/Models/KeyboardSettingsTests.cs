using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class KeyboardSettingsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var settings = new KeyboardSettings();

        settings.Mode.Should().Be(KeyboardMode.TypeText);
        settings.TextToType.Should().BeEmpty();
        settings.KeyCode.Should().Be(0);
        settings.UseCtrl.Should().BeFalse();
        settings.UseAlt.Should().BeFalse();
        settings.UseShift.Should().BeFalse();
        settings.IntervalMs.Should().Be(50);
        settings.RepeatMode.Should().Be(RepeatMode.Infinite);
        settings.RepeatCount.Should().Be(10);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var settings = new KeyboardSettings
        {
            Mode = KeyboardMode.PressKey,
            TextToType = "Hello",
            KeyCode = 0x41,
            UseCtrl = true,
            UseAlt = true,
            UseShift = true,
            IntervalMs = 100,
            RepeatMode = RepeatMode.Count,
            RepeatCount = 3
        };

        settings.Mode.Should().Be(KeyboardMode.PressKey);
        settings.TextToType.Should().Be("Hello");
        settings.KeyCode.Should().Be(0x41);
        settings.UseCtrl.Should().BeTrue();
        settings.UseAlt.Should().BeTrue();
        settings.UseShift.Should().BeTrue();
        settings.IntervalMs.Should().Be(100);
        settings.RepeatMode.Should().Be(RepeatMode.Count);
        settings.RepeatCount.Should().Be(3);
    }
}
