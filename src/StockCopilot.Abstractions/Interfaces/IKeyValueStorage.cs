namespace StockCopilot.Abstractions.Interfaces;

public interface IKeyValueStorage
{
    string? Read(string key);

    void Save(string key, string? value);
}