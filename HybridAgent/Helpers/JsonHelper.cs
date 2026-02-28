using System.Text.Json;

namespace HybridAgent.Helpers;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions Default =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
}