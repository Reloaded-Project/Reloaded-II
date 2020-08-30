using System.Linq;
using System.Text;
using Reloaded.Mod.Loader.Update.Dependency.Interfaces;

namespace Reloaded.Mod.Loader.Update.Dependency
{
    /// <summary>
    /// Creates a text log based off of the current installed dependency set.
    /// </summary>
    public static class LogBuilder
    {
        public static StringBuilder Build(DependencyChecker deps)
        {
            var builder = new StringBuilder();

            if (deps.AllAvailable)
            {
                builder.AppendLine("Everything should be a-ok!\n" +
                                   "If Reloaded is not launching, please report an issue to Github or Discord as stated in Help.html");
            }
            else
            {
                builder.AppendLine("Some dependencies are missing, here's a list of them:\n");
                foreach (var dependency in deps.Dependencies.Where(x => !x.Available))
                    LogDependency(builder, dependency);
            }

            return builder;
        }

        private static void LogDependency(StringBuilder builder, IDependency dependency)
        {
            // Write Dependency Name
            builder.AppendLine($"Name: {dependency.Name} | {(dependency.Available ? "OK" : "NOT FOUND")}");

            // Download URLs
            if (dependency.Available) 
                return;

            builder.AppendLine("Try downloading the following: ");
            foreach (var url in dependency.GetUrls())
                builder.AppendLine(url);

            builder.AppendLine("=====");
        }
    }
}
