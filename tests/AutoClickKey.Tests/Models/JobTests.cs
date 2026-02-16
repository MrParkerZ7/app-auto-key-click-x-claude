using System;
using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class JobTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var job = new Job();

        job.Id.Should().NotBe(Guid.Empty);
        job.Name.Should().Be("New Job");
        job.IsEnabled.Should().BeTrue();
        job.ProfileNames.Should().NotBeNull().And.BeEmpty();
        job.DelayBetweenProfiles.Should().Be(0);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var id = Guid.NewGuid();
        var job = new Job
        {
            Id = id,
            Name = "Test Job",
            IsEnabled = false,
            DelayBetweenProfiles = 500
        };

        job.Id.Should().Be(id);
        job.Name.Should().Be("Test Job");
        job.IsEnabled.Should().BeFalse();
        job.DelayBetweenProfiles.Should().Be(500);
    }

    [Fact]
    public void ProfileNames_CanBeModified()
    {
        var job = new Job();
        job.ProfileNames.Add("Profile1");
        job.ProfileNames.Add("Profile2");

        job.ProfileNames.Should().HaveCount(2);
        job.ProfileNames.Should().Contain("Profile1");
        job.ProfileNames.Should().Contain("Profile2");
    }

    [Fact]
    public void DelayBetweenProfiles_CannotBeNegative()
    {
        var job = new Job { DelayBetweenProfiles = -100 };

        job.DelayBetweenProfiles.Should().Be(0);
    }

    [Fact]
    public void Name_RaisesPropertyChanged()
    {
        var job = new Job();
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Job.Name))
            {
                propertyChangedRaised = true;
            }
        };

        job.Name = "Changed Name";

        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_RaisesPropertyChanged()
    {
        var job = new Job();
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Job.IsEnabled))
            {
                propertyChangedRaised = true;
            }
        };

        job.IsEnabled = false;

        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void DelayBetweenProfiles_RaisesPropertyChanged()
    {
        var job = new Job();
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Job.DelayBetweenProfiles))
            {
                propertyChangedRaised = true;
            }
        };

        job.DelayBetweenProfiles = 100;

        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void Name_DoesNotRaisePropertyChanged_WhenValueSame()
    {
        var job = new Job { Name = "Test" };
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, _) => propertyChangedRaised = true;

        job.Name = "Test";

        propertyChangedRaised.Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_DoesNotRaisePropertyChanged_WhenValueSame()
    {
        var job = new Job { IsEnabled = true };
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, _) => propertyChangedRaised = true;

        job.IsEnabled = true;

        propertyChangedRaised.Should().BeFalse();
    }

    [Fact]
    public void DelayBetweenProfiles_DoesNotRaisePropertyChanged_WhenValueSame()
    {
        var job = new Job { DelayBetweenProfiles = 100 };
        var propertyChangedRaised = false;
        job.PropertyChanged += (_, _) => propertyChangedRaised = true;

        job.DelayBetweenProfiles = 100;

        propertyChangedRaised.Should().BeFalse();
    }
}
