using System.ClientModel;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();
var app = builder.Build();

string? apiKey = builder.Configuration["GitHub:Token"];
string? endpoint = builder.Configuration["GitHub:ApiEndpoint"] ?? "https://models.github.ai/inference";
string? deploymentName = builder.Configuration["GitHub:Model"] ?? "openai/gpt-4o-mini";

// Create AI agent
ChatClient chatClient = new OpenAIClient(
    new ApiKeyCredential(apiKey!),
    new OpenAIClientOptions()
    {
        Endpoint = new Uri(endpoint)
    })
    .GetChatClient(deploymentName);

var assistantAgent = chatClient.CreateAIAgent(instructions: @"
    You are a productivity assistant that helps manage my email, calendar, and tasks.

    You have access to tools for:
    - Checking calendar availability
    - Scheduling meetings
    - Drafting and sending emails
    - Managing my to-do list

    If an email involves scheduling, check availability before drafting.");

var representativeAgent = chatClient.CreateAIAgent(instructions: @"
    You represent me professionally to coworkers.

    You can only:
    - Check general availability (free/busy only)
    - Propose meeting times
    - Create meeting requests

    You must NOT:
    - Share private meeting details
    - Share personal notes
    - Share my to-do list
    - Reveal sensitive information

    Be concise and professional.");

// Map the AG-UI agent endpoint
app.MapAGUI("/assistant", assistantAgent);
app.MapAGUI("/rep", representativeAgent);

app.Run();