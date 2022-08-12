using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Octokit;
using Reloaded.AutoIndexBuilder.Config;
using Serilog.Core;
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
    public void Pull()
    {
        using var repo = Repository;

        // Pull
        var pullOptions = new PullOptions();
        pullOptions.FetchOptions = new FetchOptions();
        pullOptions.MergeOptions = new MergeOptions();
        pullOptions.FetchOptions.CredentialsProvider = GetCredentials;
        LibGit2Sharp.Commands.Pull(repo, Signature, pullOptions);
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