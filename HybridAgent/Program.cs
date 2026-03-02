using HybridAgent.Infrastructure;
using HybridAgent.Services;
using HybridAgent.Tools;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=hybridagent.db"));

builder.Services.AddHttpClient<OllamaService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<ToolRegistry>();
builder.Services.AddScoped<ToolExecutor>();
builder.Services.AddScoped<ChatAssistant>();
builder.Services.AddScoped<ITool, DispenseTool>();
builder.Services.AddScoped<IAgent, HybridRouter>();

builder.Services.AddScoped<ToolRegistry>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<RagService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase;
});
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();