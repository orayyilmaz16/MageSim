using MageSim.Application.Simulation;
using MageSim.Domain.Abstractions;
using MageSim.Infrastructure.Conditions;
using MageSim.Infrastructure.Config;
using MageSim.Infrastructure.Time;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Dependency Injection
builder.Services.AddSingleton<Coordinator>();
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<IConditionEvaluator, DefaultConditionEvaluator>();
builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
