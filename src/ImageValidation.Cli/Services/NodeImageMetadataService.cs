using ImageValidation.Cli.Models;
using ImageValidation.Cli.Utilities;
using System.Text.Json;

namespace ImageValidation.Cli.Services;

public class NodeImageMetadataService
{
    private readonly ProcessRunner _processRunner;
    private readonly string _nodeScriptPath;

    public NodeImageMetadataService(ProcessRunner processRunner, string toolsDirectory = "tools/image-meta")
    {
        _processRunner = processRunner;
        _nodeScriptPath = Path.Combine(toolsDirectory, "image-meta.js");
    }

    public async Task<Metadata> ExtractMetadataAsync(string imagePath)
    {
        var (exitCode, output, error) = await _processRunner.RunProcessAsync(
            "node", 
            $"\"{_nodeScriptPath}\" \"{imagePath}\"", 
            Path.GetDirectoryName(_nodeScriptPath) ?? ".");

        if (exitCode != 0)
        {
            throw new Exception($"Node script failed with exit code {exitCode}: {error}");
        }

        try
        {
            var nodeMetadata = JsonSerializer.Deserialize<NodeMetadata>(output);
            if (nodeMetadata == null)
            {
                throw new Exception("Failed to parse metadata from Node script");
            }

            return new Metadata(
                nodeMetadata.Width,
                nodeMetadata.Height,
                nodeMetadata.Density,
                nodeMetadata.Format ?? "unknown"
            );
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse JSON from Node script: {ex.Message}", ex);
        }
    }

    private class NodeMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double? Density { get; set; }
        public string? Format { get; set; }
    }
}