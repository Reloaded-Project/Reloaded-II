namespace Reloaded.AutoIndexBuilder.Services;

public class GitPusherService
{
    private readonly Logger _logger;
    private readonly Settings _settings;

    private Signature Signature => new Signature("Reloaded Index Bot", "admin@sewer56.dev", DateTimeOffset.UtcNow);
    private Repository Repository
    {
        get
        {
            var repo = new Repository(_settings.GitRepoPath);
            repo.Config.Set("core.autocrlf", true);
            repo.Config.Set("core.filemode", false);
            return repo;
        }
    }

    private DateTime _nextMaintenanceDateUtc;

    public GitPusherService(Logger logger, Settings settings)
    {
        _logger = logger;
        _settings = settings;
        _nextMaintenanceDateUtc = DateTime.UtcNow.Date.AddDays(-1);
    }

    /// <summary>
    /// Pulls the current changes from git.
    /// </summary>
    public void ResetToRemote()
    {
        using var repo = Repository;

        // Fetch All
        var options = new FetchOptions()
        {
            CredentialsProvider = GetCredentials,
            Prune = true
        };
        foreach (Remote remote in repo.Network.Remotes)
        {
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            LibGit2Sharp.Commands.Fetch(repo, remote.Name, refSpecs, options, "");
        }

        // Reset to the latest commit from the tracked branch
        var trackedBranch = repo.Head.TrackedBranch;
        if (trackedBranch == null)
        {
            _logger.Error("No tracked branch found.");
            throw new InvalidOperationException("No tracked branch found.");
        }

        repo.Reset(ResetMode.Hard, trackedBranch.Tip);
    }

    /// <summary>
    /// Pushes the current repository to git.
    /// </summary>
    public void Push(string friendlyName)
    {
        using var repo = Repository;

        // Stage all changes (equivalent to 'git add -A')
        LibGit2Sharp.Commands.Stage(repo, "*");

        // Check if there are staged changes
        var changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index);
        if (changes.Any())
        {
            // Reset branch to first commit, we don't want git history.
            var branch = repo.Head;
            repo.Reset(ResetMode.Soft, branch.Commits.OrderBy(x => x.Committer.When).First());

            // Create a new commit
            repo.Commit($"[Bot] Updated: {friendlyName}", Signature, Signature);
            _logger.Information($"Created a new commit for {friendlyName}.");

            // Push to remote
            var pushOptions = new PushOptions
            {
                CredentialsProvider = GetCredentials
            };

            var remote = repo.Network.Remotes.First();
            repo.Network.Push(remote, $"+{branch.CanonicalName}:{branch.UpstreamBranchCanonicalName}", pushOptions);
            _logger.Information("Pushed changes to remote repository.");
            PerformMaintenanceIfNeeded();
        }
        else
        {
            _logger.Information("No changes to commit. Nothing to push.");
        }
    }

    /// <summary>
    /// Performs git gc if a day has passed since the last maintenance.
    /// </summary>
    private void PerformMaintenanceIfNeeded()
    {
        // Current UTC date
        var currentDateUtc = DateTime.UtcNow.Date;

        // Check if maintenance has already been performed today
        if (currentDateUtc <= _nextMaintenanceDateUtc)
            return;

        _logger.Information("Starting repository maintenance: git gc.");

        try
        {
            RunGitCommand("reflog expire --all --expire=now", _settings.GitRepoPath);
            RunGitCommand("gc", _settings.GitRepoPath);
            _logger.Information("Executed 'git gc' successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error during git gc: {ex.Message}");
        }

        // Update the last maintenance date
        _nextMaintenanceDateUtc = currentDateUtc;
        _logger.Information("Repository maintenance completed.");
    }

    /// <summary>
    /// Executes a Git command using the system's Git executable.
    /// </summary>
    /// <param name="arguments">The Git command arguments.</param>
    /// <param name="workingDirectory">The directory to execute the command in.</param>
    private void RunGitCommand(string arguments, string workingDirectory)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git", // Assumes 'git' is in the system PATH
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;

        // Start the process
        process.Start();

        // Read the output streams
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Wait for the process to exit
        process.WaitForExit();

        // Log the outputs
        if (!string.IsNullOrEmpty(output))
        {
            _logger.Information($"git output: {output}");
        }

        if (!string.IsNullOrEmpty(error))
        {
            _logger.Error($"git error: {error}");
            throw new Exception($"Git command error: {error}");
        }

        // Check the exit code
        if (process.ExitCode != 0)
        {
            throw new Exception($"Git command exited with code {process.ExitCode}");
        }
    }

    private Credentials GetCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types) => 
        new UsernamePasswordCredentials() 
        { 
            Username = _settings.GitUserName, 
            Password = _settings.GitPassword 
        };
}