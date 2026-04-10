namespace Chat.Api.Services;

public interface ISentimentAnalysisService
{
    Task<SentimentAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default);
}