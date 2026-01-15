// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.AI;

string serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:8888";

Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}\n");

// Create the AG-UI client agent
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

AGUIChatClient chatClient = new(httpClient, serverUrl);
AIAgent agent = chatClient.CreateAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent");

AgentThread thread = agent.GetNewThread();
List<ChatMessage> messages =
[
    new(ChatRole.System, "You are a helpful assistant."),
    new(ChatRole.User, "hello"),
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
        bool isFirstUpdate = true;
        string? threadId = null;

        var updates = agent.RunStreamingAsync(messages, thread);
        await foreach (AgentRunResponseUpdate update in updates)
        {
            ChatResponseUpdate chatUpdate = update.AsChatResponseUpdate();

            // First update indicates run started
            if (isFirstUpdate)
            {
                threadId = chatUpdate.ConversationId;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[Run Started - Thread: {chatUpdate.ConversationId}, Run: {chatUpdate.ResponseId}]");
                Console.ResetColor();
                isFirstUpdate = false;
            }

            // Display streaming text content
            foreach (AIContent content in update.Contents)
            {
                if (content is TextContent textContent)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(textContent.Text);
                    Console.ResetColor();
                }
                else if (content is ErrorContent errorContent)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[Error: {errorContent.Message}]");
                    Console.ResetColor();
                }
                else if (content is FunctionCallContent functionCallContent)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[Function Call: {functionCallContent.Name} with arguments {functionCallContent.Arguments}]");
                    Console.ResetColor();
                }
                else if (content is FunctionResultContent functionResultContent)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\n[Function Result: {functionResultContent.Result}]");
                    Console.ResetColor();
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"\n[Run Finished - Thread: {threadId}]");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nAn error occurred: {ex.Message}");
}