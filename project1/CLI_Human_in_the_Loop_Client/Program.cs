// Copyright (c) Microsoft. All rights reserved.

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

AIFunction sendEmailFunction = AIFunctionFactory.Create(SendEmail);
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AIFunction approvalRequiredSendEmailFunction = new ApprovalRequiredAIFunction(sendEmailFunction);
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

AGUIChatClient chatClient = new(httpClient, serverUrl);
AIAgent agent = chatClient.CreateAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent",
    tools: [approvalRequiredSendEmailFunction]);

AgentThread thread = agent.GetNewThread();
List<ChatMessage> messages =
[
    new(ChatRole.System, "You are a helpful assistant.")
];

try
{
    while (true)
    {
        // Get user input
        Console.Write("\nUser (:q or quit to exit): ");
        string? message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Request cannot be empty.");
            continue;
        }

        if (message is ":q" or "quit")
        {
            break;
        }

        messages.Add(new ChatMessage(ChatRole.User, message));

        // Stream the response
        var updates = agent.RunStreamingAsync(messages, thread);
        await foreach (AgentRunResponseUpdate update in updates)
        {
            ChatResponseUpdate chatUpdate = update.AsChatResponseUpdate();

            // Display streaming text content
            foreach (AIContent content in update.Contents)
            {
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                if (content is FunctionApprovalRequestContent approvalRequestContent)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    // print the arguments of the function call
                    
                    if (message.ToLower() is "approved")
                    {
                        var approvalMessage = new ChatMessage(ChatRole.User, [approvalRequestContent.CreateResponse(true)]);
                        Console.WriteLine(await agent.RunAsync(approvalMessage, thread));
                    }
                    else if (message.ToLower() is "denied")
                    {
                        var denialMessage = new ChatMessage(ChatRole.User, [approvalRequestContent.CreateResponse(false)]);
                        Console.WriteLine(await agent.RunAsync(denialMessage, thread));
                    }
                    else
                    {
                        var argsJson = JsonSerializer.Serialize(
                            approvalRequestContent.FunctionCall.Arguments,
                            new JsonSerializerOptions { WriteIndented = true }
                        );
                        Console.WriteLine($"\n[Function Approval Requested: {approvalRequestContent.FunctionCall.Name}\nArguments:\n{argsJson}");
                    }
                    Console.ResetColor();

                }
                if (content is TextContent textContent)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(textContent.Text);
                    Console.ResetColor();
                }
                if (content is FunctionApprovalResponseContent approvalResponseContent)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n[Function Approval Response: {(approvalResponseContent.Approved ? "Approved" : "Denied")}]");
                    Console.ResetColor();
                }
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nAn error occurred: {ex.Message}");
}