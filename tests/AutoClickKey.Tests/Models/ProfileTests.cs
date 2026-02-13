using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class ProfileTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var profile = new Profile();

        profile.Name.Should().Be("Default");
        profile.Actions.Should().NotBeNull().And.BeEmpty();
        profile.LoopActions.Should().BeTrue();
        profile.LoopCount.Should().Be(1);
        profile.DelayBetweenLoops.Should().Be(0);
        profile.ClickerSettings.Should().NotBeNull();
        profile.KeyboardSettings.Should().NotBeNull();
        profile.RecordedActions.Should().NotBeNull().And.BeEmpty();
        profile.HotkeySettings.Should().NotBeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var profile = new Profile
        {
            Name = "TestProfile",
            LoopActions = false,
            LoopCount = 5,
            DelayBetweenLoops = 1000
        };

        profile.Name.Should().Be("TestProfile");
        profile.LoopActions.Should().BeFalse();
        profile.LoopCount.Should().Be(5);
        profile.DelayBetweenLoops.Should().Be(1000);
    }

    [Fact]
    public void CreatedAt_DefaultsToCurrentTime()
    {
        var before = DateTime.Now;
        var profile = new Profile();
        var after = DateTime.Now;

        profile.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void ModifiedAt_DefaultsToCurrentTime()
    {
        var before = DateTime.Now;
        var profile = new Profile();
        var after = DateTime.Now;

        profile.ModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}

public class HotkeySettingsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var settings = new HotkeySettings();

        settings.StartStopKey.Should().Be(0x75); // F6
        settings.EmergencyStopKey.Should().Be(0x77); // F8
        settings.RecordKey.Should().Be(0x52); // R
        settings.PlayKey.Should().Be(0x50); // P
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var settings = new HotkeySettings
        {
            StartStopKey = 0x70,
            EmergencyStopKey = 0x71,
            RecordKey = 0x72,
            PlayKey = 0x73
        };

        settings.StartStopKey.Should().Be(0x70);
        settings.EmergencyStopKey.Should().Be(0x71);
        settings.RecordKey.Should().Be(0x72);
        settings.PlayKey.Should().Be(0x73);
    }
}
