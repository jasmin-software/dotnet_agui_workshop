using Client;
using Client.Components;
using Client.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

HttpClient httpClient = new();
string serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:5000";

AIFunction setBackgroundColorTool = AIFunctionFactory.Create(UserInterfaceTool.ChangeBackgroundColor);

ChatClientAgent assistantAgent = new AGUIChatClient(httpClient, $"{serverUrl}/assistant").CreateAIAgent(tools: [setBackgroundColorTool]);
ChatClientAgent representativeAgent = new AGUIChatClient(httpClient, $"{serverUrl}/rep").CreateAIAgent();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(new AgentCollection(assistantAgent, representativeAgent));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
// await app.RunAsync();