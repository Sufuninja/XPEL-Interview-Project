namespace ImageValidation.Cli.Utilities;

static class RepoPath
{
    public static string FindRepoRoot(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);

        while (dir != null)
        {
            var samples = Path.Combine(dir.FullName, "samples");
            var tools = Path.Combine(dir.FullName, "tools");

            // Look for repo markers
            if (Directory.Exists(samples) && Directory.Exists(tools))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repo root. Expected to find 'samples' and 'tools' directories when walking up from: " +
            startDirectory);
    }
}
