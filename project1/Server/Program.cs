
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;

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

// Create AI agent
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey!))
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "You're a wise motivational speaker.", name: "Socrates");

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

await app.RunAsync();

// Create simple tools
