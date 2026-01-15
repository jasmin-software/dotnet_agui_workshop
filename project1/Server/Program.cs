
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using Server.Tools;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();

WebApplication app = builder.Build();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

string? apiKey = config["GitHub:Token"];
string? endpoint = config["GitHub:ApiEndpoint"] ?? "https://models.github.ai/inference";
string? deploymentName = config["GitHub:Model"] ?? "openai/gpt-4o-mini";

AITool[] tools =
[
    AIFunctionFactory.Create(WeatherBackendTool.GetWeather)
];

// Create AI agent
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey!))
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "You're a wise motivational speaker.",
        name: "Socrates",
        tools: tools);

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

await app.RunAsync();

// Create simple tools
