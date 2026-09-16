using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
namespace Bridge.Api.Services;

public sealed class OpenAiOptions { public string ApiKey { get; set; } = ""; public string Model { get; set; } = "gpt-5.6-luna"; public string BaseUrl { get; set; } = "https://api.openai.com/v1/"; }
public interface IAiClient { Task<string> GenerateAsync(string instructions, string input, CancellationToken ct); }
public sealed class OpenAiResponsesClient(HttpClient client, IOptions<OpenAiOptions> options) : IAiClient
{
    private readonly OpenAiOptions o = options.Value;
    public async Task<string> GenerateAsync(string instructions, string input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(o.ApiKey)) throw new InvalidOperationException("AI API key is not configured.");
        using var req = new HttpRequestMessage(HttpMethod.Post, "responses"); req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", o.ApiKey); req.Content = JsonContent.Create(new { model = o.Model, instructions, input });
        using var res = await client.SendAsync(req, ct); var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode) throw new HttpRequestException("AI provider request failed.");
        using var doc = JsonDocument.Parse(body); if (doc.RootElement.TryGetProperty("output_text", out var direct)) return direct.GetString() ?? "";
        if (doc.RootElement.TryGetProperty("output", out var output)) foreach (var item in output.EnumerateArray()) if (item.TryGetProperty("content", out var content)) foreach (var part in content.EnumerateArray()) if (part.TryGetProperty("text", out var text)) return text.GetString() ?? "";
        throw new InvalidOperationException("AI response did not contain text.");
    }
}
public static class AiJson
{
    public static T Parse<T>(string value)
    {
        var json = value.Trim();
        if (json.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = json.IndexOf('\n');
            var lastFence = json.LastIndexOf("```", StringComparison.Ordinal);
            if (firstLineEnd >= 0 && lastFence > firstLineEnd)
                json = json[(firstLineEnd + 1)..lastFence].Trim();
        }
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("AI returned invalid JSON.");
    }
}
