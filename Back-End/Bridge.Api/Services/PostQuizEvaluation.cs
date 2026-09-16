using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bridge.Api.Services;

// Config for the post-quiz evaluation AI step. Same shape as PersonaAnalysis:
// give the prompt inline via "PostQuizEvaluation:SystemPrompt" or (default) read
// it from the bundled file. While no prompt resolves and no key is set, the
// endpoint returns a "pending" status instead of calling the model.
public sealed class PostQuizEvaluationOptions
{
    public string SystemPrompt { get; set; } = "";
    public string SystemPromptPath { get; set; } = "Prompts/post-quiz-evaluation-v1.txt";
    public string PromptVersion { get; set; } = "post-quiz-eval-v1";
}

public sealed class PostQuizPromptProvider(IHostEnvironment environment, IOptions<PostQuizEvaluationOptions> options)
{
    private readonly PostQuizEvaluationOptions o = options.Value;
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
