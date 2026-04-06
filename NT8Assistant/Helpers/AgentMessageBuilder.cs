using NT8Assistant.Models;

namespace NT8Assistant.Helpers;

public static class AgentMessageBuilder
{
    public static AgentMessage BuildValidationMessage(
        IEnumerable<string> missingFields,
        string userInput)
    {
        return new AgentMessage
        {
            Type = "validation",
            Content = new ValidationContent
            {
                MissingFields = missingFields.ToList(),
                OriginalUserInput = userInput,
                Reasoning = $"Missing required fields: {string.Join(", ", missingFields)}",
                FieldMetadata = BuildFieldMetadata(missingFields)
            }
        };
    }

    private static Dictionary<string, FieldMetadata> BuildFieldMetadata(IEnumerable<string> fields)
    {
        var metadata = new Dictionary<string, FieldMetadata>();

        foreach (var field in fields)
        {
            if (field.ToLower().Contains("volume"))
            {
                metadata[field] = new FieldMetadata
                {
                    Type = "number",
                    Guess = 10
                };
            }
            else if (field.ToLower().Contains("plate"))
            {
                metadata[field] = new FieldMetadata
                {
                    Type = "select",
                    Options = new List<string> { "PlateA", "PlateB", "PlateC" }
                };
            }
            else
            {
                metadata[field] = new FieldMetadata
                {
                    Type = "text"
                };
            }
        }

        return metadata;
    }
}