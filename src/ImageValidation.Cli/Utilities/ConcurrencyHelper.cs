using System.Collections.Concurrent;

namespace ImageValidation.Cli.Utilities;

public class ConcurrencyHelper
{
    private readonly SemaphoreSlim _semaphore;

    public ConcurrencyHelper(int maxConcurrency)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public async Task<T> RunWithConcurrencyLimit<T>(Func<Task<T>> taskFactory)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await taskFactory();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RunWithConcurrencyLimit(Func<Task> taskFactory)
    {
        await _semaphore.WaitAsync();
        try
        {
            await taskFactory();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<T>> ProcessCollectionWithConcurrencyLimit<T, TResult>(
        IEnumerable<T> items, 
        Func<T, Task<TResult>> processor)
    {
        var results = new ConcurrentBag<TResult>();
        
        var tasks = items.Select(async item =>
        {
            var result = await RunWithConcurrencyLimit(() => processor(item));
            results.Add(result);
        });

        await Task.WhenAll(tasks);
        return results.Cast<T>();
    }
}