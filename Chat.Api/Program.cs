using Chat.Api.Data;
using Chat.Api.Hubs;
using Chat.Api.Options;
using Chat.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.SignalR;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.Configure<AzureLanguageOptions>(
    builder.Configuration.GetSection("Azure:Language"));

builder.Services.AddScoped<ISentimentAnalysisService, SentimentAnalysisService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

builder.Services
    .AddSignalR()
    .AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat.Api v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();