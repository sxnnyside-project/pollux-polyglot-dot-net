using System.Text.Json;

namespace Sxnnyside.Pollux;

/// <summary>
/// Immutable evaluation verdict returned by PolluxEngine.
/// </summary>
/// <param name="IsAllowed">Indicates whether the operation was permitted.</param>
/// <param name="Outcome">The decision outcome status string.</param>
/// <param name="Reason">The rationale or explanation for the outcome.</param>
/// <param name="TraceJson">The raw JSON trace emitted by the engine.</param>
public record EvaluationResult(
    bool IsAllowed,
    string Outcome,
    string Reason,
    string TraceJson)
{
    /// <summary>
    /// Deserializes an evaluation result from raw trace JSON returned by Pollux Core.
    /// </summary>
    /// <param name="traceJson">Raw trace JSON string emitted by Pollux Core.</param>
    /// <returns>A parsed <see cref="EvaluationResult"/> instance.</returns>
    public static EvaluationResult FromTraceJson(string traceJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(traceJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("decision", out var decisionProp))
            {
                var outcome = "deny";
                var reason = string.Empty;

                if (decisionProp.ValueKind == JsonValueKind.Object)
                {
                    if (decisionProp.TryGetProperty("outcome", out var outProp))
                    {
                        outcome = outProp.GetString() ?? "deny";
                    }

                    if (decisionProp.TryGetProperty("reason", out var reasProp))
                    {
                        reason = reasProp.GetString() ?? string.Empty;
                    }
                }
                else if (decisionProp.ValueKind == JsonValueKind.String)
                {
                    outcome = decisionProp.GetString() ?? "deny";
                }

                var isAllowed = string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase);
                return new EvaluationResult(isAllowed, outcome, reason, traceJson);
            }
        }
        catch (JsonException)
        {
            // Fallback for non-standard or partial payloads
        }

        var fallbackAllowed = traceJson.Contains("\"outcome\":\"allow\"", StringComparison.OrdinalIgnoreCase) ||
                              traceJson.Contains("\"outcome\": \"allow\"", StringComparison.OrdinalIgnoreCase);
        var fallbackOutcome = fallbackAllowed ? "allow" : "deny";
        return new EvaluationResult(fallbackAllowed, fallbackOutcome, string.Empty, traceJson);
    }
}
