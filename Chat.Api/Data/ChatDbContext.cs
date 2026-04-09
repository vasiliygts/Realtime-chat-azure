using Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    }
}