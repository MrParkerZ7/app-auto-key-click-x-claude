using AutoClickKey.Models;
using AutoClickKey.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoClickKey.Tests.Services;

public class WorkspaceRunnerServiceTests
{
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly ProfileService _profileService;
    private readonly ActionRunnerService _actionRunner;
    private readonly WorkspaceRunnerService _service;

    public WorkspaceRunnerServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _profileService = new ProfileService(_fileSystemMock.Object, @"C:\TestProfiles");
        _actionRunner = new ActionRunnerService();
        _service = new WorkspaceRunnerService(_actionRunner, _profileService);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        _service.IsRunning.Should().BeFalse();
        _service.IsPaused.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_DoesNotRun_WhenNoJobs()
    {
        var workspace = new Workspace();
        var stoppedRaised = false;
        _service.Stopped += (_, _) => stoppedRaised = true;

        await _service.RunAsync(workspace);

        stoppedRaised.Should().BeFalse();
        _service.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_CompletesSuccessfully_WithValidWorkspace()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = false };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 1 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("TestProfile");
        var workspace = new Workspace();
        workspace.Jobs.Add(job);

        var stoppedRaised = false;
        _service.Stopped += (_, _) => stoppedRaised = true;

        await _service.RunAsync(workspace);

        stoppedRaised.Should().BeTrue();
        _service.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_SkipsDisabledJobs()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = false };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 1 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var enabledJob = new Job { Name = "EnabledJob", IsEnabled = true };
        enabledJob.ProfileNames.Add("TestProfile");
        var disabledJob = new Job { Name = "DisabledJob", IsEnabled = false };
        disabledJob.ProfileNames.Add("TestProfile");

        var workspace = new Workspace();
        workspace.Jobs.Add(disabledJob);
        workspace.Jobs.Add(enabledJob);

        var progressReports = new List<WorkspaceProgressEventArgs>();
        _service.ProgressChanged += (_, e) => progressReports.Add(e);

        await _service.RunAsync(workspace);

        progressReports.Should().Contain(p => p.CurrentJobName == "EnabledJob");
        progressReports.Should().NotContain(p => p.CurrentJobName == "DisabledJob");
    }

    [Fact]
    public async Task RunAsync_SkipsMissingProfiles()
    {
        _fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("MissingProfile");
        var workspace = new Workspace();
        workspace.Jobs.Add(job);

        var stoppedRaised = false;
        _service.Stopped += (_, _) => stoppedRaised = true;

        await _service.RunAsync(workspace);

        stoppedRaised.Should().BeTrue();
    }

    [Fact]
    public async Task Stop_StopsRunningWorkspace()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = true, LoopCount = 0 };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 1000 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("TestProfile");
        var workspace = new Workspace { LoopWorkspace = true, WorkspaceLoopCount = 0 };
        workspace.Jobs.Add(job);

        var stoppedRaised = false;
        _service.Stopped += (_, _) => stoppedRaised = true;

        var runTask = _service.RunAsync(workspace);
        await Task.Delay(50);

        _service.Stop();
        await runTask;

        stoppedRaised.Should().BeTrue();
        _service.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Pause_PausesRunningWorkspace()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = true, LoopCount = 0 };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 100 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("TestProfile");
        var workspace = new Workspace { LoopWorkspace = true, WorkspaceLoopCount = 0 };
        workspace.Jobs.Add(job);

        var pausedRaised = false;
        _service.Paused += (_, _) => pausedRaised = true;

        var runTask = _service.RunAsync(workspace);
        await Task.Delay(50);

        _service.Pause();

        pausedRaised.Should().BeTrue();
        _service.IsPaused.Should().BeTrue();

        _service.Stop();
        await runTask;
    }

    [Fact]
    public async Task Resume_ResumesAfterPause()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = true, LoopCount = 0 };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 100 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("TestProfile");
        var workspace = new Workspace { LoopWorkspace = true, WorkspaceLoopCount = 0 };
        workspace.Jobs.Add(job);

        var resumedRaised = false;
        _service.Resumed += (_, _) => resumedRaised = true;

        var runTask = _service.RunAsync(workspace);
        await Task.Delay(50);

        _service.Pause();
        await Task.Delay(20);
        _service.Resume();

        resumedRaised.Should().BeTrue();
        _service.IsPaused.Should().BeFalse();

        _service.Stop();
        await runTask;
    }

    [Fact]
    public void Pause_DoesNothing_WhenNotRunning()
    {
        var pausedRaised = false;
        _service.Paused += (_, _) => pausedRaised = true;

        _service.Pause();

        pausedRaised.Should().BeFalse();
        _service.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void Resume_DoesNothing_WhenNotPaused()
    {
        var resumedRaised = false;
        _service.Resumed += (_, _) => resumedRaised = true;

        _service.Resume();

        resumedRaised.Should().BeFalse();
    }

    [Fact]
    public async Task ProgressChanged_ReportsProgress()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = false };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 1 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var job = new Job { Name = "Job1" };
        job.ProfileNames.Add("TestProfile");
        var workspace = new Workspace();
        workspace.Jobs.Add(job);

        var progressReports = new List<WorkspaceProgressEventArgs>();
        _service.ProgressChanged += (_, e) => progressReports.Add(e);

        await _service.RunAsync(workspace);

        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(p => p.CurrentJobName == "Job1");
        progressReports.Should().Contain(p => p.CurrentProfileName == "TestProfile");
    }

    [Fact]
    public async Task RunAsync_RespectsWorkspaceLoopCount()
    {
        var profile = new Profile { Name = "TestProfile", LoopActions = false };
        profile.Actions.Add(new ActionItem { Type = ActionItemType.Delay, DelayMs = 1 });
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\TestProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\TestProfile.json")).Returns(profileJson);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("TestProfile");
        var workspace = new Workspace
        {
            LoopWorkspace = true,
            WorkspaceLoopCount = 2
        };
        workspace.Jobs.Add(job);

        var maxLoopSeen = 0;
        _service.ProgressChanged += (_, e) =>
        {
            if (e.WorkspaceLoopCount > maxLoopSeen)
            {
                maxLoopSeen = e.WorkspaceLoopCount;
            }
        };

        await _service.RunAsync(workspace);

        maxLoopSeen.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_SkipsActionsWithEmptyProfiles()
    {
        var profile = new Profile { Name = "EmptyProfile", LoopActions = false };
        var profileJson = System.Text.Json.JsonSerializer.Serialize(profile);

        _fileSystemMock.Setup(fs => fs.FileExists(@"C:\TestProfiles\EmptyProfile.json")).Returns(true);
        _fileSystemMock.Setup(fs => fs.ReadAllText(@"C:\TestProfiles\EmptyProfile.json")).Returns(profileJson);

        var job = new Job { Name = "TestJob" };
        job.ProfileNames.Add("EmptyProfile");
        var workspace = new Workspace();
        workspace.Jobs.Add(job);

        var stoppedRaised = false;
        _service.Stopped += (_, _) => stoppedRaised = true;

        await _service.RunAsync(workspace);

        stoppedRaised.Should().BeTrue();
    }
}

public class WorkspaceProgressEventArgsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var args = new WorkspaceProgressEventArgs();

        args.CurrentJobName.Should().Be(string.Empty);
        args.CurrentJobIndex.Should().Be(0);
        args.TotalJobs.Should().Be(0);
        args.WorkspaceLoopCount.Should().Be(0);
        args.CurrentProfileName.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var args = new WorkspaceProgressEventArgs
        {
            CurrentJobName = "Job1",
            CurrentJobIndex = 2,
            TotalJobs = 5,
            WorkspaceLoopCount = 3,
            CurrentProfileName = "Profile1"
        };

        args.CurrentJobName.Should().Be("Job1");
        args.CurrentJobIndex.Should().Be(2);
        args.TotalJobs.Should().Be(5);
        args.WorkspaceLoopCount.Should().Be(3);
        args.CurrentProfileName.Should().Be("Profile1");
    }
}
