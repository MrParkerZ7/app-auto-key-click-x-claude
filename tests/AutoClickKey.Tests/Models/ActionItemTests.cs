using System.ComponentModel;
using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class ActionItemTests
{
    [Fact]
    public void Clone_CreatesNewInstanceWithSameValues()
    {
        // Arrange
        var original = new ActionItem
        {
            Type = ActionItemType.Click,
            IsEnabled = true,
            MouseButton = 1,
            ClickType = 2,
            UseCurrentPosition = true,
            FixedX = 100,
            FixedY = 200,
            Key = "A",
            UseCtrl = true,
            UseAlt = true,
            UseShift = true,
            DelayMs = 500,
            RepeatCount = 3,
            Remark = "Test remark"
        };

        // Act
        var clone = (ActionItem)original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.Id.Should().NotBe(original.Id);
        clone.Type.Should().Be(original.Type);
        clone.IsEnabled.Should().Be(original.IsEnabled);
        clone.MouseButton.Should().Be(original.MouseButton);
        clone.ClickType.Should().Be(original.ClickType);
        clone.UseCurrentPosition.Should().Be(original.UseCurrentPosition);
        clone.FixedX.Should().Be(original.FixedX);
        clone.FixedY.Should().Be(original.FixedY);
        clone.Key.Should().Be(original.Key);
        clone.UseCtrl.Should().Be(original.UseCtrl);
        clone.UseAlt.Should().Be(original.UseAlt);
        clone.UseShift.Should().Be(original.UseShift);
        clone.DelayMs.Should().Be(original.DelayMs);
        clone.RepeatCount.Should().Be(original.RepeatCount);
        clone.Remark.Should().Be(original.Remark);
    }

    [Fact]
    public void Clone_GeneratesNewGuid()
    {
        // Arrange
        var original = new ActionItem();
        var originalId = original.Id;

        // Act
        var clone = (ActionItem)original.Clone();

        // Assert
        clone.Id.Should().NotBe(originalId);
    }

    [Fact]
    public void PropertyChanged_RaisedWhenPropertyChanges()
    {
        // Arrange
        var item = new ActionItem();
        var changedProperties = new List<string?>();
        item.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act
        item.IsEnabled = false;
        item.DelayMs = 200;
        item.Remark = "New remark";

        // Assert
        changedProperties.Should().Contain("IsEnabled");
        changedProperties.Should().Contain("DelayMs");
        changedProperties.Should().Contain("Remark");
    }

    [Fact]
    public void PropertyChanged_NotRaisedWhenValueIsSame()
    {
        // Arrange
        var item = new ActionItem { DelayMs = 100 };
        var changedProperties = new List<string?>();
        item.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act
        item.DelayMs = 100; // Same value

        // Assert
        changedProperties.Should().NotContain("DelayMs");
    }

    [Fact]
    public void DisplayName_Click_ShowsMouseButtonAndPosition()
    {
        // Arrange
        var item = new ActionItem
        {
            Type = ActionItemType.Click,
            MouseButton = 0,
            UseCurrentPosition = false,
            FixedX = 100,
            FixedY = 200
        };

        // Assert
        item.DisplayName.Should().Be("Click Left at (100, 200)");
    }

    [Fact]
    public void DisplayName_Click_ShowsCursorWhenUsingCurrentPosition()
    {
        // Arrange
        var item = new ActionItem
        {
            Type = ActionItemType.Click,
            MouseButton = 1,
            UseCurrentPosition = true
        };

        // Assert
        item.DisplayName.Should().Be("Click Right at cursor");
    }

    [Fact]
    public void DisplayName_KeyPress_ShowsKeyWithModifiers()
    {
        // Arrange
        var item = new ActionItem
        {
            Type = ActionItemType.KeyPress,
            Key = "A",
            UseCtrl = true,
            UseAlt = true,
            UseShift = false
        };

        // Assert
        item.DisplayName.Should().Be("Press Ctrl+Alt+A");
    }

    [Fact]
    public void DisplayName_KeyPress_ShowsKeyWithoutModifiers()
    {
        // Arrange
        var item = new ActionItem
        {
            Type = ActionItemType.KeyPress,
            Key = "Enter",
            UseCtrl = false,
            UseAlt = false,
            UseShift = false
        };

        // Assert
        item.DisplayName.Should().Be("Press Enter");
    }

    [Fact]
    public void DisplayName_Delay_ShowsDelayInMilliseconds()
    {
        // Arrange
        var item = new ActionItem
        {
            Type = ActionItemType.Delay,
            DelayMs = 1500
        };

        // Assert
        item.DisplayName.Should().Be("Wait 1500ms");
    }

    [Theory]
    [InlineData(0, "Left")]
    [InlineData(1, "Right")]
    [InlineData(2, "Middle")]
    [InlineData(99, "Left")] // Default case
    public void DisplayName_Click_ShowsCorrectMouseButtonName(int mouseButton, string expectedName)
    {
        // Arrange
        var item = new ActionItem
        {
            Type = ActionItemType.Click,
            MouseButton = mouseButton,
            UseCurrentPosition = true
        };

        // Assert
        item.DisplayName.Should().Contain($"Click {expectedName}");
    }

    [Fact]
    public void Type_Change_RaisesDisplayNamePropertyChanged()
    {
        // Arrange
        var item = new ActionItem();
        var changedProperties = new List<string?>();
        item.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act
        item.Type = ActionItemType.Delay;

        // Assert
        changedProperties.Should().Contain("Type");
        changedProperties.Should().Contain("DisplayName");
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var item = new ActionItem();

        // Assert
        item.Type.Should().Be(ActionItemType.Click);
        item.IsEnabled.Should().BeTrue();
        item.MouseButton.Should().Be(0);
        item.ClickType.Should().Be(0);
        item.UseCurrentPosition.Should().BeFalse();
        item.FixedX.Should().Be(0);
        item.FixedY.Should().Be(0);
        item.Key.Should().BeEmpty();
        item.UseCtrl.Should().BeFalse();
        item.UseAlt.Should().BeFalse();
        item.UseShift.Should().BeFalse();
        item.DelayMs.Should().Be(100);
        item.RepeatCount.Should().Be(1);
        item.Remark.Should().BeEmpty();
        item.Id.Should().NotBeEmpty();
    }
}
