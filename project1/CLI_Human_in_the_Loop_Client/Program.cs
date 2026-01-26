using System.ComponentModel;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

string serverUrl = "http://localhost:8888";

Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}\n");

// Create the AG-UI client agent
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

[Description("Send an email to a recipient.")]
static string SendEmail(
    [Description("The email address to send to")] string to,
    [Description("The subject line")] string subject,
    [Description("The email body")] string body)
{
    return $"Email sent to {to} with subject '{subject}'";
}

AIFunction sendEmailTool = AIFunctionFactory.Create(SendEmail);
AIFunction approvalRequiredSendEmailTool = new ApprovalRequiredAIFunction(sendEmailTool);

AGUIChatClient chatClient = new(httpClient, serverUrl);
AIAgent agent = chatClient.CreateAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent",
    tools: [approvalRequiredSendEmailTool]);

AgentThread thread = agent.GetNewThread();
List<ChatMessage> messages =
[
    new(ChatRole.System, "You are a helpful assistant.")
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
                if (content is FunctionApprovalRequestContent request)
                {
                    var input = message.Trim().ToLowerInvariant();
                    if (input == "approve" || input == "a" || input == "yes" || input == "y")
                    {
                        var approvalMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(true)]);
                        await HandleFunctionApprovalResponse(agent, approvalMessage);
                    }
                    else if (input == "deny" || input == "d" || input == "no" || input == "n")
                    {
                        var denialMessage = new ChatMessage(ChatRole.User, [request.CreateResponse(false)]);
                        await HandleFunctionApprovalResponse(agent, denialMessage);
                    }
                    else
                    {
                        var argsJson = JsonSerializer.Serialize(
                            request.FunctionCall.Arguments,
                            new JsonSerializerOptions { WriteIndented = true }
                        );
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"\nPlease confirm that you'd like to send the email with the following details:\n{argsJson}");
                        awaitingApproval = true;
                    }
                    Console.ResetColor();
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
    var response = message.Contents.First() as FunctionApprovalResponseContent;
    Console.ForegroundColor = response!.Approved ? ConsoleColor.Green : ConsoleColor.Red;

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