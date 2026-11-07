using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Domain.Enums;

public class SegmentSplitterTests
{
    [Fact]
    public void Long_line_is_split_into_chunks()
    {
        var geo = """{"type":"LineString","coordinates":[[28.0,-26.0],[28.0,-26.008]]}""";
        var segments = SegmentSplitter.Split(geo, 500);

        Assert.Equal(2, segments.Count);
        Assert.Equal(-26.0, segments[0].Start.Lat);
        Assert.Equal(-26.008, segments[^1].End.Lat, 6);
        Assert.InRange(segments.Sum(s => s.LengthM), 885, 895);
    }

    [Fact]
    public void Short_line_stays_one_segment()
    {
        var geo = """{"type":"LineString","coordinates":[[28.0,-26.0],[28.0,-26.001]]}""";

        Assert.Single(SegmentSplitter.Split(geo, 500));
    }
}

public class ExplanationBuilderTests
{
    [Fact]
    public void Insufficient_data_says_so()
    {
        var text = ExplanationBuilder.Build(new(AssessmentStatus.InsufficientData, null, null, new()));

        Assert.Contains("could not be completed", text);
    }

    [Fact]
    public void Valid_risk_lists_the_factors()
    {
        var text = ExplanationBuilder.Build(new(AssessmentStatus.Valid, 0.8, RiskLevel.High, new() { "Rain", "Low visibility" }));

        Assert.Contains("rain", text);
        Assert.Contains("low visibility", text);
    }
}