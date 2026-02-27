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
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<IAgentTool, CalculatorTool>();
builder.Services.AddScoped<IAgentTool, TimeTool>();

builder.Services.AddScoped<ToolRegistry>();
builder.Services.AddScoped<AgentService>();
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();