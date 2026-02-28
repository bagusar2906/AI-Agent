using System.Data;
using System.Text.Json;

namespace HybridAgent.Tools;

public class CalculatorTool : IAgentTool
{
    public string Name => "calculator";

    public string Description =>
        "Performs arithmetic calculations like addition, subtraction, multiplication, and division.";

    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            expression = new
            {
                type = "string",
                description = "Arithmetic expression like 2+2*5"
            }
        },
        required = new[] { "expression" }
    };

    public Task<string> ExecuteAsync(string argumentsJson)
    {
        var doc = JsonDocument.Parse(argumentsJson);
        var expression = doc.RootElement.GetProperty("expression").GetString();

        var result = new DataTable().Compute(expression, null);

        return Task.FromResult(result.ToString()!);
    }
}