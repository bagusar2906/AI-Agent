namespace NT8Assistant.Models;

public class FieldMetadata
{
    public string Type { get; set; } = "text"; // text | number | select
    public List<string>? Options { get; set; }
    public object? Guess { get; set; }
}