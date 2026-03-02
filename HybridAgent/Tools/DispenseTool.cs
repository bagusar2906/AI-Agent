namespace HybridAgent.Tools;

public class DispenseTool : AgentTool<DispenseArgs>
{
    public override string Name => "Dispense";

    public override object GetSchema() => new
    {
        type = "function",
        function = new
        {
            name = Name,
            description = "Dispense liquid across plate columns",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    volume = new { type = "number" },
                    plateName = new { type = "string" },
                    sourceLocation = new { type = "string" },
                    startColumn = new { type = "integer" },
                    endColumn = new { type = "integer" }
                },
                required = new[]
                {
                    "volume",
                    "plateName",
                    "sourceLocation",
                    "startColumn",
                    "endColumn"
                }
            }
        }
    };

    protected override Task<string> ExecuteAsync(DispenseArgs args)
    {
        // Real hardware logic here
        return Task.FromResult(
            $"Dispensed {args.Volume}uL from {args.SourceLocation} " +
            $"to {args.PlateName} columns {args.StartColumn}-{args.EndColumn}");
    }
}