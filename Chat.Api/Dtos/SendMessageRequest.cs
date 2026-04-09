namespace Chat.Api.Dtos
{
    public class SendMessageRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
