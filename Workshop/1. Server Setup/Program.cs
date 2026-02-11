using System.ClientModel;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Server.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();
var app = builder.Build();

string? apiKey = builder.Configuration["GitHub:Token"];
string? endpoint = builder.Configuration["GitHub:ApiEndpoint"] ?? "https://models.github.ai/inference";
string? deploymentName = builder.Configuration["GitHub:Model"] ?? "openai/gpt-4o-mini";

// Create AI agent
AITool[] tools =
[
    AIFunctionFactory.Create(WeatherBackendTool.GetWeather)
];

// Create AI agent
var agent = new OpenAIClient(
    new ApiKeyCredential(apiKey!),
    new OpenAIClientOptions()
    {
        Endpoint = new Uri(endpoint)
    })
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "You're a wise motivational speaker.",
        name: "Socrates",
        tools: tools);

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

app.Run();