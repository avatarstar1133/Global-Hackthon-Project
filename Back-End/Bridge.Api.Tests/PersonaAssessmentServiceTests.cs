using Bridge.Api.Contracts;
using Bridge.Api.Domain;
using Bridge.Api.Services;

namespace Bridge.Api.Tests;

public sealed class PersonaAssessmentServiceTests
{
    private static AssessmentDefinition Definition()
    {
        var scale = new AssessmentQuestion
        {
            Id = "q-scale", Code = "Q2", Prompt = "How confident?", AnswerType = "scale",
            Order = 1, ScaleMin = 1, ScaleMax = 5, IsRequired = true
        };
        var single = new AssessmentQuestion
        {
            Id = "q-single", Code = "Q6", Prompt = "Pick one", AnswerType = "single-choice",
            Order = 2, MinSelections = 1, MaxSelections = 1, IsRequired = true
        };
        single.Options.Add(new AssessmentOption { Id = "q6-a", QuestionId = "q-single", Value = "A", Label = "Option A", Order = 1 });
        single.Options.Add(new AssessmentOption { Id = "q6-b", QuestionId = "q-single", Value = "B", Label = "Option B", Order = 2 });
        var multi = new AssessmentQuestion
        {
            Id = "q-multi", Code = "Q10", Prompt = "Pick up to two", AnswerType = "multi-select",
            Order = 3, MinSelections = 1, MaxSelections = 2, IsRequired = false
        };
        multi.Options.Add(new AssessmentOption { Id = "q10-a", QuestionId = "q-multi", Value = "X", Label = "Anxiety X", Order = 1 });
        multi.Options.Add(new AssessmentOption { Id = "q10-b", QuestionId = "q-multi", Value = "Y", Label = "Anxiety Y", Order = 2 });
        multi.Options.Add(new AssessmentOption { Id = "q10-c", QuestionId = "q-multi", Value = "Z", Label = "Anxiety Z", Order = 3 });

        var section = new AssessmentSection { Id = "s1", Title = "Section 1", Order = 1 };
        section.Questions.AddRange([scale, single, multi]);
        scale.Section = single.Section = multi.Section = section;

        var definition = new AssessmentDefinition { Id = "persona-v1", Name = "Persona", Version = "v1" };
        definition.Sections.Add(section);
        section.AssessmentDefinition = definition;
        return definition;
    }

    private static readonly PersonaAssessmentService Service = new();

    [Fact]
    public void Validate_AcceptsWellFormedAnswers()
    {
        var answers = new List<PersonaAnswerInput>
        {
            new("q-scale", null, 4),
            new("q-single", ["A"], null),
            new("q-multi", ["X", "Z"], null)
        };
        var normalized = Service.Validate(Definition(), answers);
        Assert.Equal(3, normalized.Count);
        Assert.Equal(4, normalized[0].ScaleValue);
        Assert.Equal(["Anxiety X", "Anxiety Z"], normalized[2].SelectedLabels);
    }

    [Fact]
    public void Validate_AllowsSkippingOptionalQuestion()
    {
        var answers = new List<PersonaAnswerInput>
        {
            new("q-scale", null, 3),
            new("q-single", ["B"], null)
        };
        var normalized = Service.Validate(Definition(), answers);
        Assert.Equal(2, normalized.Count);
    }

    [Fact]
    public void Validate_RejectsMissingRequiredQuestion()
    {
        var answers = new List<PersonaAnswerInput> { new("q-scale", null, 3) };
        Assert.Throws<ArgumentException>(() => Service.Validate(Definition(), answers));
    }

    [Fact]
    public void Validate_RejectsScaleOutOfRange()
    {
        var answers = new List<PersonaAnswerInput>
        {
            new("q-scale", null, 9),
            new("q-single", ["A"], null)
        };
        Assert.Throws<ArgumentException>(() => Service.Validate(Definition(), answers));
    }

    [Fact]
    public void Validate_RejectsUnknownOption()
    {
        var answers = new List<PersonaAnswerInput>
        {
            new("q-scale", null, 3),
            new("q-single", ["Z"], null)
        };
        Assert.Throws<ArgumentException>(() => Service.Validate(Definition(), answers));
    }

    [Fact]
    public void Validate_RejectsTooManySelections()
    {
        var answers = new List<PersonaAnswerInput>
        {
            new("q-scale", null, 3),
            new("q-single", ["A"], null),
            new("q-multi", ["X", "Y", "Z"], null)
        };
        Assert.Throws<ArgumentException>(() => Service.Validate(Definition(), answers));
    }

    [Fact]
    public void Validate_RejectsUnknownQuestion()
    {
        var answers = new List<PersonaAnswerInput>
        {
            new("q-scale", null, 3),
            new("q-single", ["A"], null),
            new("q-ghost", ["A"], null)
        };
        Assert.Throws<ArgumentException>(() => Service.Validate(Definition(), answers));
    }

    [Fact]
    public void BuildAnalysisInput_IncludesPromptsAndAnswers()
    {
        var definition = Definition();
        var normalized = Service.Validate(definition, new List<PersonaAnswerInput>
        {
            new("q-scale", null, 4),
            new("q-single", ["A"], null)
        });
        var input = Service.BuildAnalysisInput(definition, normalized);
        Assert.Contains("How confident?", input);
        Assert.Contains("4/5", input);
        Assert.Contains("Option A", input);
        Assert.Contains("(skipped)", input); // optional multi question left unanswered
    }
}
