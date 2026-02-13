using AutoClickKey.Helpers;
using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class RecordedActionTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var action = new RecordedAction();

        action.Type.Should().Be(ActionType.MouseClick);
        action.Timestamp.Should().Be(0);
        action.DelayFromPrevious.Should().Be(0);
        action.X.Should().Be(0);
        action.Y.Should().Be(0);
        action.MouseButton.Should().Be(MouseButton.Left);
        action.KeyCode.Should().Be(0);
        action.IsKeyDown.Should().BeFalse();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var action = new RecordedAction
        {
            Type = ActionType.KeyPress,
            Timestamp = 1234567890,
            DelayFromPrevious = 100,
            X = 500,
            Y = 300,
            MouseButton = MouseButton.Middle,
            KeyCode = 0x41,
            IsKeyDown = true
        };

        action.Type.Should().Be(ActionType.KeyPress);
        action.Timestamp.Should().Be(1234567890);
        action.DelayFromPrevious.Should().Be(100);
        action.X.Should().Be(500);
        action.Y.Should().Be(300);
        action.MouseButton.Should().Be(MouseButton.Middle);
        action.KeyCode.Should().Be(0x41);
        action.IsKeyDown.Should().BeTrue();
    }

    [Theory]
    [InlineData(ActionType.MouseClick)]
    [InlineData(ActionType.MouseMove)]
    [InlineData(ActionType.KeyPress)]
    public void Type_CanBeSetToAllValues(ActionType actionType)
    {
        var action = new RecordedAction { Type = actionType };
        action.Type.Should().Be(actionType);
    }
}
