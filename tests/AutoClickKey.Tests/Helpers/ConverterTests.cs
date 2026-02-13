using System.Globalization;
using System.Windows;
using AutoClickKey.Helpers;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Helpers;

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter _converter = new ();

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
    private readonly StartStopTextConverter _converter = new ();

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
}

public class InverseBoolToVisibilityConverterTests
{
    private readonly InverseBoolToVisibilityConverter _converter = new ();

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
}

public class RecordButtonTextConverterTests
{
    private readonly RecordButtonTextConverter _converter = new ();

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
}

public class StringToVisibilityConverterTests
{
    private readonly StringToVisibilityConverter _converter = new ();

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
}

public class IndexToVisibilityConverterTests
{
    private readonly IndexToVisibilityConverter _converter = new ();

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
}
