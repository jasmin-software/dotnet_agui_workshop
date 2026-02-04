# Human-in-the-Loop

### Creating tools that requires approval/human in the loop:

> [!TIP]
>
> `FunctionApprovalRequestContent` and `ApprovalRequiredAIFunction` are meant for evaluation and may change or be removed in future updates. They will be highlighted with a warning (similar to a syntax error) in your editor. 
>
> To ignore this warning, create an `.editorconfig` file in the project root folder with the following content: 
>
> ```
> [*.cs]
> # MEAI001: Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
> dotnet_diagnostic.MEAI001.severity = none
> ```

add this tool to the Program.cs in the client folder:
``` C#
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
```

make it an `AIFunction` that requires approval by wrapping it around `ApprovalRequiredAIFunction`:
``` C#
AIFunction approvalRequiredSendEmailTool = new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SendEmail));
```

add the tool to the agent:
``` C#
AIAgent agent = chatClient.CreateAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent",
    tools: [setTextColorTool, 
            generateTextFileTool]);
```

create a helper function that handles the approval response:
``` c#
async Task HandleFunctionApprovalResponse(AIAgent agent, ChatMessage message)
{
    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(message))
    {
        Console.Write(update.Text);
    }
    awaitingApproval = false;
}
```

add this else-if condition to the `AIContent` foreach loop to take care of the function approval:
``` C#
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
```

### Using tools with approval:
run this to start the client again:

```
dotnet run
```

And you can simply ask it to create a text file for you

<details>
<summary>
here's an example of the interaction:
</summary>

![human-in-the-loop light themed output](./assets/hitl-output-light.png#gh-light-mode-only)

![human-in-the-loop dark themed output](./assets/hitl-output-dark.png#gh-dark-mode-only)
</details>


### What's happening?

```mermaid
sequenceDiagram;
   User->>Client:   0. Send message to create text file
   Client->>Server: 1. Start streaming request
   activate Server
   Server->>Client: 2. Request approval
   Client->>User: 3. Prompt for approval
   User->>Client: 4. Reply (approve/deny)
   Client->>Server: 5. Send approval response
   Server->>Client: 6. Request to call GenerateTextFile
   Note left of Client: 7 Execute GenerateTextFile
   Client->>Server: 8. Return result
   Server->>Client: 9. Stream SSE response
   deactivate Server
   Client->>User: 10. Display message
```

When you send a message to generate a text file:
1. The client sends the request to the server via HTTP (`RunStreamingAsync`).
2. The server requests tool execution approval (`FunctionApprovalRequestContent`) from the client.
3. The client prompts the user for approval.
4. The user approves or denies the request.
5. The client converts the user's decision into a `FunctionApprovalResponseContent` and sends it back to the server.
6. The server sends the tool call request to the client.
7. The client executes `GenerateTextFile` with arguments provided by the server.
8. The client sends the result of `GenerateTextFile` back to the server.
9. The server incorporates the result into the agent response and streams it back to the client via SSE.
10. The client displays the message to you.