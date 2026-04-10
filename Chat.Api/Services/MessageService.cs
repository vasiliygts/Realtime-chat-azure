using Chat.Api.Data;
using Chat.Api.Dtos;
using Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;
        private readonly ISentimentAnalysisService _sentimentAnalysisService;

        public MessageService(ChatDbContext context,
            ISentimentAnalysisService sentimentAnalysisService)
        {
            _context = context;
            _sentimentAnalysisService = sentimentAnalysisService;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync()
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .OrderBy(m => m.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<ChatMessage?> CreateMessageAsync(SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Text))
            {
                return null;
            }

            string trimmedUserName = request.UserName.Trim();
            string trimmedText = request.Text.Trim();

            SentimentAnalysisResult sentimentResult =
                await _sentimentAnalysisService.AnalyzeAsync(trimmedText);

            ChatMessage message = new ChatMessage
            {
                UserName = trimmedUserName,
                Text = trimmedText,
                CreatedAtUtc = DateTime.UtcNow,
                Sentiment = sentimentResult.Sentiment,
                SentimentScore = sentimentResult.SentimentScore
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }
    }
}