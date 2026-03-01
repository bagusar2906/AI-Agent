using System.Text.Json;
using HybridAgent.Helpers;

namespace HybridAgent.Tools;

public class DispenseTool : IAgentTool
{
    public string Name => "dispense";

    public string Description =>
        "Dispense liquid into a plate across a column range.";

    public object ParametersSchema
    {
        get
        {
            return new
            {
                type = "object",
                properties = new
                {
                    volume = new
                    {
                        type = "number",
                        description = "Volume to dispense in µL"
                    },
                    plateName = new
                    {
                        type = "string",
                        description = "Target plate name"
                    },
                    sourceLocation = new
                    {
                        type = "string",
                        description = "Source reservoir location"
                    },
                    startColumn = new
                    {
                        type = "integer",
                        description = "Starting column number"
                    },
                    endColumn = new
                    {
                        type = "integer",
                        description = "Ending column number"
                    }
                },
                required = new[]
                {
                    "volume",
                    "plateName",
                    "sourceLocation",
                    "startColumn",
                    "endColumn"
                }
            };
        }
    }

    public Task<string> ExecuteAsync(string argumentsJson)
    {
        var args = JsonSerializer.Deserialize<DispenseArgs>(argumentsJson, JsonHelper.Default)!;

        if (args.StartColumn > args.EndColumn)
            throw new Exception("Start column must be <= end column");

        // Simulated execution logic
        var result =
            $"Dispensing {args.Volume}µL from {args.SourceLocation} " +
            $"into plate {args.PlateName} " +
            $"from column {args.StartColumn} to {args.EndColumn}.";

        return Task.FromResult(result);
    }

    private class DispenseArgs
    {
        public double Volume { get; set; }
        public string PlateName { get; set; } = "";
        public string SourceLocation { get; set; } = "";
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
    }
}