namespace NT8Assistant.Models;

public class ValidationContent
{
    public List<string> MissingFields { get; set; } = new();
    public string OriginalUserInput { get; set; } = "";
    public string Reasoning { get; set; } = "";
    public Dictionary<string, FieldMetadata>? FieldMetadata { get; set; }
}