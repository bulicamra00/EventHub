using EventHub.Domain.Interfaces;
using System.Text;

namespace EventHub.Infrastructure.Services;

public class CsvService : ICsvService
{
    public byte[] ExportAttendeesToCsv<T>(IEnumerable<T> data)
    {
        var builder = new StringBuilder();
        var properties = typeof(T).GetProperties();

        builder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
            builder.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}