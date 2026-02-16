using System;
using System.Collections.Generic;

namespace AutoClickKey.Models;

public class Workspace
{
    public string Name { get; set; } = "New Workspace";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime ModifiedAt { get; set; } = DateTime.Now;

    public List<Job> Jobs { get; set; } = new();

    public bool LoopWorkspace { get; set; }

    public int WorkspaceLoopCount { get; set; } = 1;

    public int DelayBetweenJobs { get; set; }
}
