using Azure;
using Azure.AI.TextAnalytics;
using Chat.Api.Options;
using Microsoft.Extensions.Options;

namespace Chat.Api.Services;

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly TextAnalyticsClient _client;
    private readonly ILogger<SentimentAnalysisService> _logger;

    public SentimentAnalysisService(
        IOptions<AzureLanguageOptions> options,
        ILogger<SentimentAnalysisService> logger)
    {
        _logger = logger;

        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException("Azure:Language:Endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("Azure:Language:ApiKey is not configured.");
        }

        _client = new TextAnalyticsClient(
            new Uri(settings.Endpoint),
            new AzureKeyCredential(settings.ApiKey));
    }

    public async Task<SentimentAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new SentimentAnalysisResult();
        }

        try
        {
            var response = await _client.AnalyzeSentimentAsync(text, cancellationToken: cancellationToken);



            decimal? score = response.Value.Sentiment switch
            {
                TextSentiment.Positive => (decimal)response.Value.ConfidenceScores.Positive,
                TextSentiment.Negative => (decimal)response.Value.ConfidenceScores.Negative,
                TextSentiment.Neutral => (decimal)response.Value.ConfidenceScores.Neutral,
                TextSentiment.Mixed => (decimal)response.Value.ConfidenceScores.Positive,
                _ => (decimal?)null
            };

            // те саме 

            //double? score = null;

            //if (response.Value.Sentiment == TextSentiment.Positive)
            //{
            //    score = response.Value.ConfidenceScores.Positive;
            //}
            //else if (response.Value.Sentiment == TextSentiment.Negative)
            //{
            //    score = response.Value.ConfidenceScores.Negative;
            //}
            //else if (response.Value.Sentiment == TextSentiment.Neutral)
            //{
            //    score = response.Value.ConfidenceScores.Neutral;
            //}
            //else if (response.Value.Sentiment == TextSentiment.Mixed)
            //{
            //    score = response.Value.ConfidenceScores.Positive;
            //}

            return new SentimentAnalysisResult
            {
                Sentiment = response.Value.Sentiment.ToString().ToLowerInvariant(),
                SentimentScore = score
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sentiment analysis failed for message text.");
            return new SentimentAnalysisResult();
        }
    }
}