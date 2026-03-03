namespace NT8Assistant.Tools;

public class DispenseArgs
{
    public double? Volume { get; set; }
    public string? Station { get; set; }
    public string? SourceLocation { get; set; }
    public int? StartColumn { get; set; }
    public int? EndColumn { get; set; }
}