using AutoClickKey.Helpers;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Helpers;

public class RelayCommandTests
{
    [Fact]
    public void Execute_CallsExecuteAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute(null);

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithParameter_PassesParameter()
    {
        // Arrange
        object? receivedParameter = null;
        var command = new RelayCommand(param => receivedParameter = param);

        // Act
        command.Execute("test value");

        // Assert
        receivedParameter.Should().Be("test value");
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenNoCanExecuteProvided()
    {
        // Arrange
        var command = new RelayCommand(() => { });

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_ReturnsFalse_WhenCanExecuteReturnsFalse()
    {
        // Arrange
        var command = new RelayCommand(() => { }, () => false);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanExecute_ReturnsTrue_WhenCanExecuteReturnsTrue()
    {
        // Arrange
        var command = new RelayCommand(() => { }, () => true);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithPredicate_EvaluatesParameter()
    {
        // Arrange
        var command = new RelayCommand(
            _ => { },
            param => param is int value && value > 5);

        // Act & Assert
        command.CanExecute(10).Should().BeTrue();
        command.CanExecute(3).Should().BeFalse();
        command.CanExecute("not an int").Should().BeFalse();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenExecuteIsNull()
    {
        // Act
        Action act = () => new RelayCommand((Action<object?>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("execute");
    }

    [Fact]
    public void Execute_WithTypedAction_ExecutesCorrectly()
    {
        // Arrange
        var executedValue = 0;
        var command = new RelayCommand(param =>
        {
            if (param is int value)
            {
                executedValue = value;
            }
        });

        // Act
        command.Execute(42);

        // Assert
        executedValue.Should().Be(42);
    }

    [Fact]
    public void RaiseCanExecuteChanged_DoesNotThrow()
    {
        // Arrange
        var command = new RelayCommand(() => { });

        // Act
        Action act = () => command.RaiseCanExecuteChanged();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CanExecuteChanged_CanSubscribeAndUnsubscribe()
    {
        // Arrange
        var command = new RelayCommand(() => { });
        var eventRaised = false;
        EventHandler handler = (_, _) => eventRaised = true;

        // Act - Subscribe
        command.CanExecuteChanged += handler;

        // Act - Unsubscribe
        command.CanExecuteChanged -= handler;

        // Assert - Just verify no exceptions were thrown
        eventRaised.Should().BeFalse();
    }
}
