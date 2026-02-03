using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

string serverUrl = "http://localhost:5000";
ConsoleColor currentColor = Console.ForegroundColor;

Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}\n");

// Create the AG-UI client agent
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

[Description("Change the console foreground color into the specified color.")]
void ChangeConsoleForegroundColor(string color)
{
    if (Enum.TryParse<ConsoleColor>(color, out var parsedColor))
    {
        currentColor = parsedColor;
        Console.ForegroundColor = parsedColor;
    }
    else //should I throw instead?
    {
        currentColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.White;
    }
}

[Description("Send an email to a recipient.")]
static string SendEmail(
    [Description("The email address to send to")] string to,
    [Description("The subject line")] string subject,
    [Description("The email body")] string body)
{
    return $"Email sent to {to} with subject '{subject}'";
}

AIFunction changeConsoleForegroundColorTool = AIFunctionFactory.Create(ChangeConsoleForegroundColor);
AIFunction approvalRequiredSendEmailTool = new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SendEmail));

AGUIChatClient chatClient = new(httpClient, serverUrl);
AIAgent agent = chatClient.CreateAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent",
    tools: [
        changeConsoleForegroundColorTool, 
        approvalRequiredSendEmailTool]);

AgentThread thread = agent.GetNewThread();
List<ChatMessage> messages =
[
    new(ChatRole.System, "You are a helpful assistant."),
    new(ChatRole.System, "If you're asked to return a color for the console foreground, return from the enum of ConsoleColor, with CamelCase."),
];

bool awaitingApproval = false;
string regularPrompt = "\n> ";
string approvalPrompt = "\nApprove execution? (approve/deny): ";
Console.Write("\nEnter your message or :q to quit.\n");

try
{
    while (true)
    {
        // Get user input
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

        // Stream the response
        var updates = agent.RunStreamingAsync(messages, thread);
        await foreach (AgentRunResponseUpdate update in updates)
        {
            // Display streaming text content
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
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\n[Function Call: {functionCallContent.Name}]\nArguments:\n{argsJson}");
                    Console.ForegroundColor = currentColor;
                }
                else if (content is FunctionResultContent functionResultContent)
                {
                    Console.WriteLine($"\n[Function Result: {functionResultContent.Result}]");
                }                
                else if (content is FunctionApprovalRequestContent request)
                {
                    var input = message.Trim().ToLowerInvariant();
                    if (input == "approve" || input == "a" || input == "yes" || input == "y")
                    {
                        var approvalMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(true)]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        await HandleFunctionApprovalResponse(agent, approvalMessage);
                        Console.ForegroundColor = currentColor;
                    }
                    else if (input == "deny" || input == "d" || input == "no" || input == "n")
                    {
                        var denialMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(false)]);
                        Console.ForegroundColor = ConsoleColor.Red;
                        await HandleFunctionApprovalResponse(agent, denialMessage);
                        Console.ForegroundColor = currentColor;
                    }
                    else
                    {
                        var argsJson = JsonSerializer.Serialize(
                            request.FunctionCall.Arguments,
                            new JsonSerializerOptions { WriteIndented = true }
                        );
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nPlease confirm that you'd like to send the email with the following details:\n{argsJson}");
                        Console.ForegroundColor = currentColor;
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

async Task HandleFunctionApprovalResponse(AIAgent agent, ChatMessage message)
{
    var updates = agent.RunStreamingAsync(message);

    await foreach (AgentRunResponseUpdate update in updates)
    {
        foreach (AIContent content in update.Contents)
        {
            if (content is TextContent textContent)
            {
                Console.Write(textContent.Text);
            }
        }
    }
    awaitingApproval = false;
}