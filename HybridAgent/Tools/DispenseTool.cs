namespace HybridAgent.Tools;

public class DispenseTool : Tool<List<DispenseArgs>>
{
    public override string Name => "Dispense";

    /*public override object GetSchema() => new
    {
        type = "function",
        function = new
        {
            name = Name,
            description = "Dispense liquid across plate columns",
            parameters = new
            {
                type = "array",
                items = new [] {
                 new {
                     type = "object",
                    properties = new
                    {
                        volume = new { type = "number" },
                        station = new { type = "string" },
                        sourceLocation = new { type = "string" },
                        startColumn = new { type = "integer" },
                        endColumn = new { type = "integer" }
                    }  },
                },
                required = new[]
                {
                    "volume",
                    "station",
                    "sourceLocation",
                    "startColumn",
                    "endColumn"
                }
            }
        }
    };*/

    public override object GetSchema() => new
    {

                items = new [] {
                    new {
                        type = "object",
                        properties = new
                        {
                            volume = new { type = "number" },
                            station = new { type = "string" },
                            sourceLocation = new { type = "string" },
                            startColumn = new { type = "integer" },
                            endColumn = new { type = "integer" }
                        }  },
                },
                required = new[]
                {
                    "volume",
                    "station",
                    "sourceLocation",
                    "startColumn",
                    "endColumn"
                }
    };

    protected override async Task<IToolResult> ExecuteAsync(List<DispenseArgs> args)
    {
        // Real hardware logic here
        // if (args.Volume <= 0)
        //     return ToolResult.Fail(
        //         "dispense",
        //         "Volume must be greater than zero.",
        //         "INVALID_VOLUME");

        // Execute hardware logic here
       // await _robot.DispenseAsync(args);

        return  ToolResult.Ok(
            "dispense",
            "Dispense operation completed successfully.",
           args);
    }
}