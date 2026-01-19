using System.ComponentModel;
using System.Net.Http.Json;
using Blazor_Client.Components.Pages;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

namespace Blazor_Client.Services;

public interface IAGUIAgentService
{
    AIAgent Agent {get;}

    Task InitializeAsync();
    IAsyncEnumerable<string> StreamMessageAsync(string message);
    Task<string> SendMessageAsync(string message);
    Task RunAsync(string message);

}

public class AGUIAgentService : IAGUIAgentService
{
    private readonly HttpClient _httpClient;
    private AGUIChatClient? _chatClient;    
    private AIAgent? _agent;

    public AIAgent Agent => _agent ?? throw new InvalidOperationException("Agent not initialized. Call InitializeAsync() first.");

    public AGUIAgentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [Description("Change the background color of the chat interface to the specified color.")]
    private static Task ChangeBackgroundColor(string color)
    {
        AiChat.Colour = color;
        return Task.CompletedTask;
    }
    // Example 2 is to show toast notifications

    AITool[] tools = [
        AIFunctionFactory.Create(ChangeBackgroundColor)
    ];

    public Task InitializeAsync()
    {
        _chatClient = new AGUIChatClient(_httpClient, _httpClient.BaseAddress!.ToString());
        _agent = _chatClient.CreateAIAgent(
            name: "agui-client",
            description: "AG-UI Client Agent",
            tools: tools);
        return Task.CompletedTask;
    }

    public async Task RunAsync(string message)
    {
        if (_agent == null)
            throw new InvalidOperationException("Agent not initialized.");

        await _agent.RunAsync(message);
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(string message)
    {
        if (_agent == null)
            throw new InvalidOperationException("Agent not initialized.");

        await foreach (var update in _agent.RunStreamingAsync(message))
        {
            foreach (var content in update.Contents)
            {
                if (content is TextContent textContent)
                    yield return textContent.Text;
            }
        }
    }

    public async Task<string> SendMessageAsync(string message)
    {
        var result = "";
        if (_agent == null)
            throw new InvalidOperationException("Agent not initialized.");

        await foreach (var update in  _agent.RunStreamingAsync(message)) {
            foreach (var content in update.Contents)
            {
                if (content is TextContent textContent)
                    result += textContent.Text;
            }
        }
        return result;
    }
}
