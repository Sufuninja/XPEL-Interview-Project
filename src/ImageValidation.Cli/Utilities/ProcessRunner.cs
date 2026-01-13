using System.Diagnostics;

namespace ImageValidation.Cli.Utilities;

public class ProcessRunner
{
    public async Task<(int ExitCode, string Output, string Error)> RunProcessAsync(
        string fileName, 
        string arguments, 
        string workingDirectory)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        
        try
        {
            process.Start();
            
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            var output = await outputTask;
            var error = await errorTask;
            
            return (process.ExitCode, output, error);
        }
        catch (Exception ex)
        {
            return (-1, string.Empty, $"Failed to start process: {ex.Message}");
        }
    }
}