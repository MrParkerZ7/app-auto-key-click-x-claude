using System;
using AutoClickKey.Models;
using FluentAssertions;
using Xunit;

namespace AutoClickKey.Tests.Models;

public class WorkspaceTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var workspace = new Workspace();

        workspace.Name.Should().Be("New Workspace");
        workspace.Jobs.Should().NotBeNull().And.BeEmpty();
        workspace.LoopWorkspace.Should().BeFalse();
        workspace.WorkspaceLoopCount.Should().Be(1);
        workspace.DelayBetweenJobs.Should().Be(0);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var workspace = new Workspace
        {
            Name = "Test Workspace",
            LoopWorkspace = true,
            WorkspaceLoopCount = 5,
            DelayBetweenJobs = 1000
        };

        workspace.Name.Should().Be("Test Workspace");
        workspace.LoopWorkspace.Should().BeTrue();
        workspace.WorkspaceLoopCount.Should().Be(5);
        workspace.DelayBetweenJobs.Should().Be(1000);
    }

    [Fact]
    public void CreatedAt_DefaultsToCurrentTime()
    {
        var before = DateTime.Now;
        var workspace = new Workspace();
        var after = DateTime.Now;

        workspace.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void ModifiedAt_DefaultsToCurrentTime()
    {
        var before = DateTime.Now;
        var workspace = new Workspace();
        var after = DateTime.Now;

        workspace.ModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Jobs_CanBeModified()
    {
        var workspace = new Workspace();
        var job1 = new Job { Name = "Job 1" };
        var job2 = new Job { Name = "Job 2" };

        workspace.Jobs.Add(job1);
        workspace.Jobs.Add(job2);

        workspace.Jobs.Should().HaveCount(2);
        workspace.Jobs.Should().Contain(job1);
        workspace.Jobs.Should().Contain(job2);
    }

    [Fact]
    public void CreatedAt_CanBeSet()
    {
        var customDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var workspace = new Workspace { CreatedAt = customDate };

        workspace.CreatedAt.Should().Be(customDate);
    }

    [Fact]
    public void ModifiedAt_CanBeSet()
    {
        var customDate = new DateTime(2024, 2, 20, 14, 45, 0);
        var workspace = new Workspace { ModifiedAt = customDate };

        workspace.ModifiedAt.Should().Be(customDate);
    }
}
