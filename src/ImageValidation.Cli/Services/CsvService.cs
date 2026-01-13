using CsvHelper;
using ImageValidation.Cli.Models;
using System.Globalization;

namespace ImageValidation.Cli.Services;

public class CsvService
{
    public async Task<IEnumerable<Sku>> ReadSkusFromCsvAsync(string filePath)
    {
        var records = new List<Sku>();
        
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        await csv.ReadAsync();
        csv.ReadHeader();
        
        while (await csv.ReadAsync())
        {
            var skuValue = csv.GetField("SKU");
            if (!string.IsNullOrWhiteSpace(skuValue))
            {
                records.Add(new Sku(skuValue));
            }
        }
        
        return records;
    }
    
    public async Task WriteReportToCsvAsync<T>(string filePath, IEnumerable<T> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        await csv.WriteRecordsAsync(records);
    }
}