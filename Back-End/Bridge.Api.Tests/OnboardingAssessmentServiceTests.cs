using Bridge.Api.Contracts;
using Bridge.Api.Services;

namespace Bridge.Api.Tests;

public sealed class OnboardingAssessmentServiceTests
{
    [Fact]
    public void Assess_ComputesRoundedConfidenceAndKeepsSelectedFocus()
    {
        var service = new OnboardingAssessmentService();
        var request = new OnboardingAssessmentRequest(
            "Minh", "Vietnam", "first-semester", 2, 3, 4, "professor");
        var result = service.Assess(request);
        Assert.Equal(3, result.BaselineConfidence);
        Assert.Equal("professor", result.RecommendedActorType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Assess_RejectsComfortOutsideOneToFive(int invalidScore)
    {
        var service = new OnboardingAssessmentService();
        var request = new OnboardingAssessmentRequest(
            "Minh", "Vietnam", "first-semester", invalidScore, 3, 3, "friend");
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Assess(request));
    }
    [Fact]
    public void Assess_RejectsRemovedColleagueActor()
    {
        var service = new OnboardingAssessmentService();
        var request = new OnboardingAssessmentRequest(
            "Minh", "Vietnam", "first-semester", 3, 3, 3, "colleague");

        Assert.Throws<ArgumentException>(() => service.Assess(request));
    }
}