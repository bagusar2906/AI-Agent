using HybridAgent.Infrastructure;
using HybridAgent.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=hybridagent.db"));

builder.Services.AddHttpClient<OllamaService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<RagService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.MapControllers();
app.Run();