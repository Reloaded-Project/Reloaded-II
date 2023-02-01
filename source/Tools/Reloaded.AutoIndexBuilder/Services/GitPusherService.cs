using Credentials = LibGit2Sharp.Credentials;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

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

    public GitPusherService(Logger logger, Settings settings)
    {
        _logger = logger;
        _settings = settings;
    }


    /// <summary>
    /// Pulls the current changes from git.
    /// </summary>
    public void ResetToRemote()
    {
        using var repo = Repository;

        // Fetch All
        var options = new FetchOptions() { CredentialsProvider = GetCredentials };
        foreach (Remote remote in repo.Network.Remotes)
        {
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            LibGit2Sharp.Commands.Fetch(repo, remote.Name, refSpecs, options, "");
        }
        
        // Reset
        var trackedBranch = repo.Head.TrackedBranch;
        repo.Reset(ResetMode.Hard, trackedBranch.Commits.OrderByDescending(x => x.Committer.When).First());
    }

    /// <summary>
    /// Pushes the current repository to git.
    /// </summary>
    public void Push()
    {
        using var repo = Repository;

        // Select branch
        var branch = repo.Head;

        // Reset soft.
        repo.Reset(ResetMode.Soft, branch.Commits.OrderBy(x => x.Committer.When).First());

        // git add -A
        LibGit2Sharp.Commands.Stage(repo, "*");

        // git commit -m
        repo.Commit("[Bot] Update Search Index", Signature, Signature);

        // git push
        var pushOptions = new PushOptions
        {
            CredentialsProvider = GetCredentials
        };

        var remote = repo.Network.Remotes.First();
        repo.Network.Push(remote, $"+{branch.CanonicalName}:{branch.UpstreamBranchCanonicalName}", pushOptions);
    }

    private Credentials GetCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types) => new UsernamePasswordCredentials() { Username = _settings.GitUserName, Password = _settings.GitPassword };
}