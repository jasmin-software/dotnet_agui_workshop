using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

string serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:5000";
Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}\n");

AIFunction setTextColorTool = AIFunctionFactory.Create(SetTextColor);
AIFunction generateTextFileTool = new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GenerateTextFile) );

// Create the AG-UI client agent
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

AGUIChatClient chatClient = new(httpClient, serverUrl);

AIAgent agent = chatClient.AsAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent",
    tools: [setTextColorTool,
            generateTextFileTool]);

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
                else if (content is FunctionApprovalRequestContent request)
                {
                    var input = message.Trim().ToLowerInvariant();
                    if (input == "approve" || input == "a" || input == "yes" || input == "y")
                    {
                        var approvalMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(true)]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        await HandleFunctionApprovalResponse(agent, approvalMessage);
                        Console.ForegroundColor = currentTextColor;
                    }
                    else if (input == "deny" || input == "d" || input == "no" || input == "n")
                    {
                        var denialMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(false)]);
                        Console.ForegroundColor = ConsoleColor.Red;
                        await HandleFunctionApprovalResponse(agent, denialMessage);
                        Console.ForegroundColor = currentTextColor;
                    }
                    else
                    {
                        var argsJson = JsonSerializer.Serialize(
                            request.FunctionCall.Arguments,
                            new JsonSerializerOptions { WriteIndented = true }
                        );
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nPlease confirm that you'd like to create the text file with the following details:\n{argsJson}");
                        Console.ForegroundColor = currentTextColor;
                        awaitingApproval = true;
                    }
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

[Description("Generate a text file with the specified filename and content.")]
string GenerateTextFile(
    [Description("The filename to generate")] string filename,
    [Description("The content to write to the file")] string content)
{
    string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    string filePath = Path.Combine(projectRoot, filename);

    File.WriteAllText(filePath, content);

    return $"File written to: {filePath}";
}

async Task HandleFunctionApprovalResponse(AIAgent agent, ChatMessage message)
{
    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(message))
    {
        Console.Write(update.Text);
    }
    awaitingApproval = false;
}