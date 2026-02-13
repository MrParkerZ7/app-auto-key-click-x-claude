using AutoClickKey.Helpers;
using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class ClickerSettingsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var settings = new ClickerSettings();

        settings.MouseButton.Should().Be(MouseButton.Left);
        settings.ClickType.Should().Be(ClickType.Single);
        settings.IntervalMs.Should().Be(100);
        settings.PositionMode.Should().Be(PositionMode.CurrentPosition);
        settings.FixedX.Should().Be(0);
        settings.FixedY.Should().Be(0);
        settings.RepeatMode.Should().Be(RepeatMode.Infinite);
        settings.RepeatCount.Should().Be(10);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var settings = new ClickerSettings
        {
            MouseButton = MouseButton.Right,
            ClickType = ClickType.Double,
            IntervalMs = 500,
            PositionMode = PositionMode.FixedPosition,
            FixedX = 100,
            FixedY = 200,
            RepeatMode = RepeatMode.Count,
            RepeatCount = 5
        };

        settings.MouseButton.Should().Be(MouseButton.Right);
        settings.ClickType.Should().Be(ClickType.Double);
        settings.IntervalMs.Should().Be(500);
        settings.PositionMode.Should().Be(PositionMode.FixedPosition);
        settings.FixedX.Should().Be(100);
        settings.FixedY.Should().Be(200);
        settings.RepeatMode.Should().Be(RepeatMode.Count);
        settings.RepeatCount.Should().Be(5);
    }
}
