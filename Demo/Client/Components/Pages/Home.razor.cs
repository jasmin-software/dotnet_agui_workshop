using Markdig;
using Microsoft.Extensions.AI;

namespace Client.Components.Pages;

public partial class Home(AgentCollection agentCollection)
{

    private string CurrentMessage = "";
     private List<ChatMessage> Messages = new();
    public string? Color { get; set; }

    private async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(CurrentMessage))
        {
            var userMessage = new ChatMessage
            {
                Text = CurrentMessage,
                IsUser = true
            };

            Messages.Add(userMessage);

            var userText = CurrentMessage;
            CurrentMessage = "";

            Messages.Add(new ChatMessage
            {
                Text = "",
                IsUser = false
            });

            // Simulate async bot reply
            await foreach (var update in agentCollection.AssistantAgent.RunStreamingAsync(userText))
            {
                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        Messages.Last().Text += textContent.Text;
                        StateHasChanged();
                    }
                }
            }
        }
    }

    private string RenderMarkdown(string markdown)
    {
        return Markdown.ToHtml(markdown);
    }

    public class ChatMessage
    {
        public required string Text { get; set; }
        public bool IsUser { get; set; }
    }
}