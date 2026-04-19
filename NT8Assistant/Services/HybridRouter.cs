using System.Text.RegularExpressions;
using NT8Assistant.Models;

namespace NT8Assistant.Services;

public class HybridRouter(
    ToolExecutor toolExecutor,
    ChatAssistant chatAssistant) : IAgent
{
    public IAsyncEnumerable<string> RunAsync(
        string userMessage,
        CancellationToken ct = default)
    {
        return IsToolIntent(userMessage) ? toolExecutor.RunAsync(userMessage, ct) : chatAssistant.RunAsync(userMessage, ct);
    }

    private ContextInfo DetectContext(string input)
    {
        var lower = input.ToLower();

        return new ContextInfo
        {
            NeedsStation = lower.Contains("station") && !HasStation(input),
            NeedsDate = lower.Contains("on") && !HasDate(input)
        };
    }

    private bool HasStation(string input)
    {
        return DomainFields.Stations.Any(s =>
            input.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    private bool HasDate(string input)
    {
        return Regex.IsMatch(input, @"\d{4}-\d{2}-\d{2}");
    }
    public string GenerateSuggestion(string input)
    {
        var context = DetectContext(input);

        // 🔥 Station suggestion
            if (context.NeedsStation)
        {
            var match = DomainFields.Stations
                .FirstOrDefault(s => s.StartsWith(GetLastWord(input), StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return match.Substring(GetLastWord(input).Length);
            }
        }

        // 🔥 Date suggestion
        if (context.NeedsDate)
        {
            return " " + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        }

        return "";
    }

    private string GetLastWord(string input)
    {
        var parts = input.Split(' ');
        return parts.Last();
    }

    private bool IsToolIntent(string message)
    {
        var keywords = new[]
        {
            "dispense",
            "aspirate",
            "simulate",
            "station",
            "column"
        };

        return keywords.Any(k =>
            message.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}