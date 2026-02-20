using System.Text.Json;
using Markdig;
using Microsoft.Extensions.AI;


namespace Client.Components.Pages;

public partial class Home(AgentCollection agentCollection)
{
    private string CurrentMessage = "";
     private List<ChatText> Messages = new();
    public static string? Color { get; set; }
    private bool awaitingApproval = false;
    private FunctionApprovalRequestContent? awaitingRequest;

    public class ChatText
    {
        public required string Text { get; set; }
        public bool IsUser { get; set; }
    }

    private async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(CurrentMessage))
        {
            var userMessage = new ChatText
            {
                Text = CurrentMessage,
                IsUser = true
            };

            Messages.Add(userMessage);

            var userText = CurrentMessage;
            CurrentMessage = "";

            if (awaitingApproval && awaitingRequest != null)
            {
                var input = userText.Trim().ToLowerInvariant();
                if (input == "approve" || input == "a" || input == "yes" || input == "y")
                {
                    var approvalMessage = new ChatMessage(ChatRole.User, [awaitingRequest!.CreateResponse(true)]);
                    await HandleFunctionApprovalResponse(approvalMessage);
                }
                else if (input == "deny" || input == "d" || input == "no" || input == "n")
                {
                    var denialMessage = new ChatMessage(ChatRole.User, [awaitingRequest!.CreateResponse(false)]);
                    await HandleFunctionApprovalResponse(denialMessage);
                }
                return;
            }

            Messages.Add(new ChatText
            {
                Text = "",
                IsUser = false
            });

            await foreach (var update in agentCollection.AssistantAgent.RunStreamingAsync(userText))
            {
                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        Messages.Last().Text += textContent.Text;
                        StateHasChanged();
                    }
                    else if (content is FunctionApprovalRequestContent request)
                    {
                        // var input = userText.Trim().ToLowerInvariant();
                        
                        // var argsJson = JsonSerializer.Serialize(
                        //     request.FunctionCall.Arguments,
                        //     new JsonSerializerOptions { WriteIndented = true }
                        // );
                        request.FunctionCall.Arguments!.TryGetValue("filename", out var filename);
                        request.FunctionCall.Arguments!.TryGetValue("content", out var generatedContent);
                        var msg2 = $"**Please confirm that you'd like to create the text file with the following details:**\n\nFilename:{filename}\n\nContent:\n\n{generatedContent}\n\nReply **approve** to proceed or **deny** to reject.";
                        Messages.Last().Text = msg2;
                        StateHasChanged();
                        awaitingApproval = true;
                        awaitingRequest = request;
                    }
                    
                }
            }
        }
    }

    private string RenderMarkdown(string markdown)
    {
        return Markdown.ToHtml(markdown);
    }

    async Task HandleFunctionApprovalResponse(ChatMessage message)
    {
        Messages.Add(new ChatText
        {
            Text = "",
            IsUser = false
        });
        await foreach (var update in agentCollection.AssistantAgent.RunStreamingAsync(message))
        {
            Messages.Last().Text += update.Text;
            StateHasChanged();
        }
        awaitingApproval = false;
    }
}