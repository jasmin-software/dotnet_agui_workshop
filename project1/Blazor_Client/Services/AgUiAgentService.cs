using System.ComponentModel;
using System.Net.Http.Json;
using Blazor_Client.Components.Pages;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

namespace Blazor_Client.Services;

public interface IAgUiAgentService
{
    AIAgent Agent {get;}

    Task InitializeAsync();
    IAsyncEnumerable<string> StreamMessageAsync(string message);
    Task<string> SendMessageAsync(string message);
    Task RunAsync(string message);

}

public class AgUiAgentService : IAgUiAgentService
{
    private readonly HttpClient _httpClient;
    private AGUIChatClient? _chatClient;    
    private AIAgent? _agent;

    public AIAgent Agent => _agent ?? throw new InvalidOperationException("Agent not initialized. Call InitializeAsync() first.");

    public AgUiAgentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Define a frontend function tool
    [Description("Change the backgound color of the web page to the specified color.")]
    static void ChangeBackgroundColor(string color)
    {
        AiChat.Colour = color;
    }

    public Task InitializeAsync()
    {
        _chatClient = new AGUIChatClient(_httpClient, _httpClient.BaseAddress!.ToString());
        _agent = _chatClient.CreateAIAgent(
            name: "agui-client",
            description: "AG-UI Client Agent",
            tools: [AIFunctionFactory.Create(ChangeBackgroundColor)]);
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
