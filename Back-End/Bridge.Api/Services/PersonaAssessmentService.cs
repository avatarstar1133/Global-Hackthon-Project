using System.Text;
using Bridge.Api.Contracts;
using Bridge.Api.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bridge.Api.Services;

// Configuration for the persona-analysis AI step. The system prompt can be given
// inline (via appsettings/user-secrets "PersonaAnalysis:SystemPrompt") or, more
// practically for a long prompt, read from a file at SystemPromptPath. While no
// prompt resolves, submissions are still saved and the attempt is marked
// "pending" instead of calling the model.
public sealed class PersonaAnalysisOptions
{
    public string SystemPrompt { get; set; } = "";
    public string SystemPromptPath { get; set; } = "Prompts/persona-analysis-v1.txt";
    public string PromptVersion { get; set; } = "persona-analysis-v1";
}

// Resolves the effective system prompt: inline config wins; otherwise the file
// at SystemPromptPath (relative to the content root) is read once and cached.
public sealed class PersonaPromptProvider(IHostEnvironment environment, IOptions<PersonaAnalysisOptions> options)
{
    private readonly PersonaAnalysisOptions o = options.Value;
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

public sealed record PersonaNormalizedAnswer(
    AssessmentQuestion Question,
    IReadOnlyList<string> SelectedValues,
    IReadOnlyList<string> SelectedLabels,
    int? ScaleValue);

// Pure, DB-free validation and prompt-input building so it can be unit tested.
public sealed class PersonaAssessmentService
{
    public IReadOnlyList<PersonaNormalizedAnswer> Validate(
        AssessmentDefinition definition,
        IReadOnlyList<PersonaAnswerInput> answers)
    {
        ArgumentNullException.ThrowIfNull(definition);
        answers ??= [];

        var questions = definition.Sections
            .SelectMany(section => section.Questions)
            .ToDictionary(question => question.Id, StringComparer.Ordinal);

        var answerByQuestion = new Dictionary<string, PersonaAnswerInput>(StringComparer.Ordinal);
        foreach (var answer in answers)
        {
            if (string.IsNullOrWhiteSpace(answer.QuestionId) || !questions.ContainsKey(answer.QuestionId))
                throw new ArgumentException($"Unknown question '{answer.QuestionId}'.");
            if (!answerByQuestion.TryAdd(answer.QuestionId, answer))
                throw new ArgumentException($"Duplicate answer for question '{answer.QuestionId}'.");
        }

        var normalized = new List<PersonaNormalizedAnswer>();
        foreach (var question in questions.Values.OrderBy(q => q.Section.Order).ThenBy(q => q.Order))
        {
            answerByQuestion.TryGetValue(question.Id, out var answer);
            var hasContent = answer is not null &&
                (answer.ScaleValue.HasValue || (answer.SelectedValues?.Count ?? 0) > 0);

            if (!hasContent)
            {
                if (question.IsRequired)
                    throw new ArgumentException($"Question '{question.Code}' is required.");
                continue;
            }

            normalized.Add(question.AnswerType == "scale"
                ? NormalizeScale(question, answer!)
                : NormalizeChoice(question, answer!));
        }

        return normalized;
    }

    private static PersonaNormalizedAnswer NormalizeScale(AssessmentQuestion question, PersonaAnswerInput answer)
    {
        var min = question.ScaleMin ?? 1;
        var max = question.ScaleMax ?? 5;
        if (answer.ScaleValue is not int value || value < min || value > max)
            throw new ArgumentException($"Question '{question.Code}' expects a value between {min} and {max}.");
        return new PersonaNormalizedAnswer(question, [], [], value);
    }

    private static PersonaNormalizedAnswer NormalizeChoice(AssessmentQuestion question, PersonaAnswerInput answer)
    {
        var selected = (answer.SelectedValues ?? []).Distinct(StringComparer.Ordinal).ToList();
        if (selected.Count != (answer.SelectedValues?.Count ?? 0))
            throw new ArgumentException($"Question '{question.Code}' has duplicate selections.");
        if (selected.Count < question.MinSelections || selected.Count > question.MaxSelections)
            throw new ArgumentException(
                $"Question '{question.Code}' expects between {question.MinSelections} and {question.MaxSelections} selections.");

        var optionByValue = question.Options.ToDictionary(o => o.Value, o => o.Label, StringComparer.Ordinal);
        var labels = new List<string>();
        foreach (var value in selected)
        {
            if (!optionByValue.TryGetValue(value, out var label))
                throw new ArgumentException($"Question '{question.Code}' has no option '{value}'.");
            labels.Add(label);
        }

        return new PersonaNormalizedAnswer(question, selected, labels, null);
    }

    // Readable transcript of the questionnaire + the learner's answers, passed as
    // the model input alongside the configured system prompt.
    public string BuildAnalysisInput(AssessmentDefinition definition, IReadOnlyList<PersonaNormalizedAnswer> answers)
    {
        var byQuestion = answers.ToDictionary(a => a.Question.Id, StringComparer.Ordinal);
        var builder = new StringBuilder();
        builder.AppendLine($"Assessment: {definition.Name} ({definition.Version})");

        foreach (var section in definition.Sections.OrderBy(s => s.Order))
        {
            builder.AppendLine();
            builder.AppendLine($"# {section.Title}");
            foreach (var question in section.Questions.OrderBy(q => q.Order))
            {
                builder.AppendLine($"[{question.Code}] {question.Prompt}");
                if (!byQuestion.TryGetValue(question.Id, out var answer))
                {
                    builder.AppendLine("Answer: (skipped)");
                    continue;
                }

                if (answer.ScaleValue is int scale)
                {
                    var scaleText = $"{scale}/{question.ScaleMax ?? 5}";
                    if (!string.IsNullOrWhiteSpace(question.ScaleMinLabel) || !string.IsNullOrWhiteSpace(question.ScaleMaxLabel))
                        scaleText += $" ({question.ScaleMinLabel} … {question.ScaleMaxLabel})";
                    builder.AppendLine($"Answer: {scaleText}");
                }
                else
                {
                    builder.AppendLine($"Answer: {string.Join("; ", answer.SelectedLabels)}");
                }
            }
        }

        return builder.ToString().TrimEnd();
    }
}
