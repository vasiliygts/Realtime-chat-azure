using Chat.Api.Dtos;
using Chat.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task SendMessage(SendMessageRequest request)
        {
            var message = await _messageService.CreateMessageAsync(request);

            if (message == null)
            {
                return;
            }

            await Clients.All.SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.UserName,
                message.Text,
                message.CreatedAtUtc,
                message.Sentiment,
                message.SentimentScore
            });
        }
    }
}