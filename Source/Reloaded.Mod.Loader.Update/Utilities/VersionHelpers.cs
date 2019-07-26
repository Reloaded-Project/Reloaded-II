using System;
using System.Collections.Generic;
using System.Text;
using Octokit;

namespace Reloaded.Mod.Loader.Update.Utilities
{
    public static class VersionHelpers
    {
        public static Version GithubReleaseToVersion(Release release)
        {
            return Version.Parse(release.TagName);
        }
    }
}
