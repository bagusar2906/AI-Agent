namespace HybridAgent.Tools;

public class DispenseTool : Tool<DispenseArgs>
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

    protected override async Task<IToolResult> ExecuteAsync(DispenseArgs args)
    {
        // Real hardware logic here
        if (args.Volume <= 0)
            return ToolResult.Fail(
                "dispense",
                "Volume must be greater than zero.",
                "INVALID_VOLUME");

        // Execute hardware logic here
       // await _robot.DispenseAsync(args);

        return  ToolResult.Ok(
            "dispense",
            "Dispense operation completed successfully.",
            new
            {
                args.PlateName,
                args.Volume,
                args.SourceLocation,
                args.StartColumn,
                args.EndColumn,
                Status = "Completed",
                Timestamp = DateTime.UtcNow
            });
    }
}