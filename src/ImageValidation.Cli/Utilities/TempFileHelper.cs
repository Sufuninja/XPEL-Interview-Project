namespace ImageValidation.Cli.Utilities;

public class TempFileHelper : IDisposable
{
    private readonly List<string> _tempFiles = new();
    private bool _disposed = false;

    public string CreateTempFile(string extension = ".tmp")
    {
        var tempPath = Path.GetTempFileName();
        var newTempPath = Path.ChangeExtension(tempPath, extension);
        
        // Rename the file to have the correct extension
        File.Move(tempPath, newTempPath);
        
        _tempFiles.Add(newTempPath);
        return newTempPath;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            foreach (var tempFile in _tempFiles)
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch
                {
                    // Ignore errors when cleaning up temp files
                }
            }
            _tempFiles.Clear();
            _disposed = true;
        }
    }
}