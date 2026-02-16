using System.Globalization;
using System.Windows;
using AutoClickKey.Helpers;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Helpers;

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter _converter = new();

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Convert_InvertsBoolValue(bool input, bool expected)
    {
        var result = _converter.Convert(input, typeof(bool), null!, CultureInfo.InvariantCulture);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBack_InvertsBoolValue(bool input, bool expected)
    {
        var result = _converter.ConvertBack(input, typeof(bool), null!, CultureInfo.InvariantCulture);
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_ReturnsOriginalValue_WhenNotBool()
    {
        var result = _converter.Convert("string value", typeof(bool), null!, CultureInfo.InvariantCulture);
        result.Should().Be("string value");
    }

    [Fact]
    public void ConvertBack_ReturnsOriginalValue_WhenNotBool()
    {
        var result = _converter.ConvertBack(42, typeof(bool), null!, CultureInfo.InvariantCulture);
        result.Should().Be(42);
    }
}

public class StartStopTextConverterTests
{
    private readonly StartStopTextConverter _converter = new();

    [Fact]
    public void Convert_ReturnsStop_WhenRunning()
    {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("Stop (F4)");
    }

    [Fact]
    public void Convert_ReturnsStart_WhenNotRunning()
    {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("Start (F4)");
    }

    [Fact]
    public void Convert_ReturnsStart_WhenNotBool()
    {
        var result = _converter.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("Start (F4)");
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack("Start (F4)", typeof(bool), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

public class InverseBoolToVisibilityConverterTests
{
    private readonly InverseBoolToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_ReturnsCollapsed_WhenTrue()
    {
        var result = _converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_ReturnsVisible_WhenFalse()
    {
        var result = _converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_ReturnsVisible_WhenNotBool()
    {
        var result = _converter.Convert("string", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

public class RecordButtonTextConverterTests
{
    private readonly RecordButtonTextConverter _converter = new();

    [Fact]
    public void Convert_ReturnsStopRecording_WhenRecording()
    {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("Stop Recording");
    }

    [Fact]
    public void Convert_ReturnsRecord_WhenNotRecording()
    {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("Record");
    }

    [Fact]
    public void Convert_ReturnsRecord_WhenNotBool()
    {
        var result = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("Record");
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack("Record", typeof(bool), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

public class StringToVisibilityConverterTests
{
    private readonly StringToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_ReturnsVisible_WhenStringHasContent()
    {
        var result = _converter.Convert("hello", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Convert_ReturnsCollapsed_WhenStringIsNullOrWhitespace(string? input)
    {
        var result = _converter.Convert(input!, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_ReturnsCollapsed_WhenNotString()
    {
        var result = _converter.Convert(123, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(string), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

public class IndexToVisibilityConverterTests
{
    private readonly IndexToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_ReturnsVisible_WhenIndexMatchesParameter()
    {
        var result = _converter.Convert(2, typeof(Visibility), "2", CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_ReturnsCollapsed_WhenIndexDoesNotMatchParameter()
    {
        var result = _converter.Convert(1, typeof(Visibility), "2", CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_ReturnsVisible_WhenParameterIsInvalid()
    {
        var result = _converter.Convert(1, typeof(Visibility), "not a number", CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_ReturnsVisible_WhenValueIsNotInt()
    {
        var result = _converter.Convert("string", typeof(Visibility), "2", CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(int), "2", CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

public class BoolToWorkspaceWidthConverterTests
{
    private readonly BoolToWorkspaceWidthConverter _converter = new();

    [Fact]
    public void Convert_ReturnsWidth350_WhenTrue()
    {
        var result = _converter.Convert(true, typeof(GridLength), null!, CultureInfo.InvariantCulture);
        result.Should().Be(new GridLength(350));
    }

    [Fact]
    public void Convert_ReturnsWidth0_WhenFalse()
    {
        var result = _converter.Convert(false, typeof(GridLength), null!, CultureInfo.InvariantCulture);
        result.Should().Be(new GridLength(0));
    }

    [Fact]
    public void Convert_ReturnsWidth0_WhenNotBool()
    {
        var result = _converter.Convert("string", typeof(GridLength), null!, CultureInfo.InvariantCulture);
        result.Should().Be(new GridLength(0));
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack(new GridLength(350), typeof(bool), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

public class NullToCollapsedConverterTests
{
    private readonly NullToCollapsedConverter _converter = new();

    [Fact]
    public void Convert_ReturnsCollapsed_WhenNull()
    {
        var result = _converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_ReturnsVisible_WhenNotNull()
    {
        var result = _converter.Convert("any value", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_ReturnsVisible_WhenObject()
    {
        var result = _converter.Convert(new object(), typeof(Visibility), null!, CultureInfo.InvariantCulture);
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack(Visibility.Visible, typeof(object), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}
