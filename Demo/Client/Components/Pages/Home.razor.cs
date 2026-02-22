using System.Runtime.Serialization;
using System.Text.Json;
using BlazorBootstrap;
using Markdig;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.AI;


namespace Client.Components.Pages;

public partial class Home(AgentCollection agentCollection)
{
    private string CurrentMessage = "";
    private static List<ToastMessage> toastMessages = new List<ToastMessage>();
     private List<ChatText> Messages = new ();
    public static string? Color { get; set; }
    private bool awaitingApproval = false;
    private FunctionApprovalRequestContent? awaitingRequest;
    private static bool isVerbose = false;


    public class ChatText
    {
        public required string Text { get; set; }
        public bool IsUser { get; set; }
        public bool IsApprovalRequest { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(1000);
        Messages.Add(new ChatText
            {
                Text = @"
### Welcome! 👋

Here’s what you can try out with me, your AI assistant:

1. **Change the Chat Background**
   - You can modify the chat’s background color to whatever you like, e.g., red, the color of grass, or just describe your vibe!

2. **Verbose Logging**
   - Turn on the **verbose flag** to see which functions the agent is calling. Notice the toast notifications popping up!

3. **Human-in-the-Loop file generation**
   - Ask me to **create a text file**. You’ll get the **Approve/Deny buttons** to control whether it actually executes!

---

Give them a try. Feel free to experiment with combinations of these features!",
                IsUser = false
            });
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

            Messages.Add(new ChatText
            {
                Text = "",
                IsUser = false
            });

            // await Task.Delay(300);

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
                        request.FunctionCall.Arguments!.TryGetValue("filename", out var filename);
                        request.FunctionCall.Arguments!.TryGetValue("content", out var generatedContent);
                        var msg = $"**Please confirm that you'd like to create the text file with the following details:**\n\nFilename:{filename}\n\nContent:\n\n{generatedContent}";
                        Messages.Last().Text = msg;
                        Messages.Last().IsApprovalRequest = true;
                        StateHasChanged();
                        awaitingApproval = true;
                        awaitingRequest = request;
                    }
                    if (isVerbose)
                    {
                        if (content is FunctionCallContent functionCallContent)
                        {
                            var args = string.Join("\n", functionCallContent.Arguments!.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                            var msg = 
$@"```
[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VERBOSE] Function call: {functionCallContent.Name}
Arguments:
{args}
[END VERBOSE LOG]
```";
                            Messages.Last().Text += "\n\n" + msg + "\n\n";
                            StateHasChanged();
                        }
                        else if (content is FunctionResultContent functionResultContent)
                        {
                            var msg = 
$@"```
[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VERBOSE] Function result: {functionResultContent.Result}
[END VERBOSE LOG]
```";
                            Messages.Last().Text += "\n\n" + msg + "\n\n";
                            StateHasChanged();
                        }
                    }
                }
            }
        }
    }
    private const string VerboseAlreadyMessageTemplate = "Verbose logging is already {0}. No changes were made.";
    private const string VerboseNowMessageTemplate = "Verbose logging is now {0}.";
    public static void HandleVerboseToggle(bool isVerbose)
    {
        string status = isVerbose ? "enabled" : "disabled";
        bool isNoChange = Home.isVerbose == isVerbose;
        string template = isNoChange ? VerboseAlreadyMessageTemplate : VerboseNowMessageTemplate;
        ToastType type = isNoChange ? ToastType.Warning : ToastType.Success;

        if (!isNoChange)
            Home.isVerbose = isVerbose;

        ShowMessage(isVerbose, type, string.Format(template, status));
    }


    private static void ShowMessage(bool isVerbose, ToastType type, string message) => toastMessages.Add(CreateToastMessage(isVerbose, type, message));
    
    private static ToastMessage CreateToastMessage(bool isVerbose, ToastType type, string message)
        => new ToastMessage
        {
            Type = type,
            Title = type == ToastType.Warning 
                ? "Verbose Logging is already " + (isVerbose ? "Enabled" : "Disabled")
                : "Verbose Logging " + (isVerbose ? "Enabled" : "Disabled"),

            HelpText = $"{DateTime.Now}",

            Message = type == ToastType.Warning
                ? string.Format(VerboseAlreadyMessageTemplate, message)
            : string.Format(VerboseNowMessageTemplate, message),

            AutoHide = true
        };

    private string RenderMarkdown(string markdown)
    {
        return Markdig.Markdown.ToHtml(markdown);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" 
            && !awaitingApproval 
            && !string.IsNullOrWhiteSpace(CurrentMessage))
        {
            await SendMessage();
        }
    }

    private async Task HandleApproval(bool approved)
    {
        if (awaitingRequest == null)
            return;

        var approvalMessage = new ChatMessage(
            ChatRole.User,
            [awaitingRequest.CreateResponse(approved)]
        );

        awaitingApproval = false;

        await HandleFunctionApprovalResponse(approvalMessage);
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