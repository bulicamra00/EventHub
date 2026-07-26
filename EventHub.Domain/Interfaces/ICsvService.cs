namespace EventHub.Domain.Interfaces;

public interface ICsvService
{
    byte[] ExportAttendeesToCsv<T>(IEnumerable<T> data);
}