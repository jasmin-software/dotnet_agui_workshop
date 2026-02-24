using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

string serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:5000";
Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}\n");

AIFunction setTextColorTool = AIFunctionFactory.Create(SetTextColor);

// Create the AG-UI client agent
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

AGUIChatClient chatClient = new(httpClient, serverUrl);

AIAgent agent = chatClient.AsAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent",
    tools: [setTextColorTool]);

List<ChatMessage> messages =
[
    new(ChatRole.System, "When asked to return a color for the console text, choose the closest one from the ConsoleColor enum and return with CamelCase."),
];
AgentSession session = await agent.GetNewSessionAsync();

ConsoleColor currentTextColor = Console.ForegroundColor;
Console.Write("\nEnter your message or :q to quit.\n");
string regularPrompt = "\n> ";
string approvalPrompt = "\nApprove execution? (approve/deny): ";
bool awaitingApproval = false;

try
{
    while (true)
    {
        // Get and validate user input
        Console.Write(awaitingApproval ? approvalPrompt : regularPrompt);
        string? message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Request cannot be empty.");
            continue;
        }
        if (message.ToLowerInvariant() is ":q" or "quit")
        {
            break;
        }

        messages.Add(new ChatMessage(ChatRole.User, message));

        // Stream and print the response
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(messages, session))
        {
            foreach (AIContent content in update.Contents)
            {
                if (content is TextContent textContent)
                {
                    Console.Write(textContent.Text);
                }
                else if (content is FunctionCallContent functionCallContent)
                {                    
                    var argsJson = JsonSerializer.Serialize(
                        functionCallContent.Arguments,
                        new JsonSerializerOptions { WriteIndented = true }
                    );
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\n[Function Call: {functionCallContent.Name}]\nArguments:\n{argsJson}");
                    Console.ForegroundColor = currentTextColor;
                }
                else if (content is FunctionResultContent functionResultContent)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\n[Function Result: {functionResultContent.Result}]");
                    Console.ForegroundColor = currentTextColor;
                }    
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nAn error occurred: {ex.Message}");
}

[Description("Change the console text color into the specified color.")]
string SetTextColor(string color)
{
    if (Enum.TryParse<ConsoleColor>(color, out var parsedColor))
    {
        Console.ForegroundColor = parsedColor;
        currentTextColor = parsedColor;
        return $"Console text color changed to {parsedColor}.";
    }
    else
    {
        throw new ArgumentException($"Invalid console colour '{color}'", nameof(color));
    }
}