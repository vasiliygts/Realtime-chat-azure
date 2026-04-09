using Chat.Api.Data;
using Chat.Api.Dtos;
using Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Services
{
    public class MessageService : IMessageService
    {
        private readonly ChatDbContext _context;

        public MessageService(ChatDbContext context)
        {
            _context = context;
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

            ChatMessage message = new ChatMessage
            {
                UserName = request.UserName.Trim(),
                Text = request.Text.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                Sentiment = null,
                SentimentScore = null
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }
    }
}