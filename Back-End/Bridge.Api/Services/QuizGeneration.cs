using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bridge.Api.Services;

// Config for the AI quiz generator (prompt A). Same shape as the other prompts:
// inline via "QuizGeneration:SystemPrompt" or a file at SystemPromptPath. The
// file does not exist yet, so until the prompt is supplied the /quiz endpoint
// falls back to the static QuizCatalog.
public sealed class QuizGenerationOptions
{
    public string SystemPrompt { get; set; } = "";
    public string SystemPromptPath { get; set; } = "Prompts/quiz-generation-v1.txt";
    public string PromptVersion { get; set; } = "quiz-generation-v1";
}

public sealed class QuizGenPromptProvider(IHostEnvironment environment, IOptions<QuizGenerationOptions> options)
{
    private readonly QuizGenerationOptions o = options.Value;
    private string? cached;

    public string PromptVersion => o.PromptVersion;

    public string? GetSystemPrompt()
    {
        if (!string.IsNullOrWhiteSpace(o.SystemPrompt)) return o.SystemPrompt;
        if (cached is not null) return cached.Length == 0 ? null : cached;
        if (string.IsNullOrWhiteSpace(o.SystemPromptPath)) return null;

        var path = Path.IsPathRooted(o.SystemPromptPath)
            ? o.SystemPromptPath
            : Path.Combine(environment.ContentRootPath, o.SystemPromptPath);

        cached = File.Exists(path) ? File.ReadAllText(path).Trim() : "";
        return cached.Length == 0 ? null : cached;
    }
}
