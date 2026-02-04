
using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
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

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AITool[] tools =
[
    // new ApprovalRequiredAIFunction(AIFunctionFactory.Create(WeatherBackendTool.GetWeather)),
    AIFunctionFactory.Create(WeatherBackendTool.GetWeather),
    AIFunctionFactory.Create(RecipeBackendTool.UpdateRecipe)
];
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Create AI agent // non-shared-state agent
AIAgent agent = new OpenAIClient(
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

app.MapAGUI("/", agent);

await app.RunAsync();
