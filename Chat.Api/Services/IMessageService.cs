using Chat.Api.Dtos;
using Chat.Api.Models;

namespace Chat.Api.Services
{
    public interface IMessageService
    {
        Task<List<ChatMessage>> GetMessagesAsync();
        Task<ChatMessage?> CreateMessageAsync(SendMessageRequest request);
    }
}