using System.ComponentModel;
using Client.Components.Pages;

namespace Client.Tools
{
    internal static class UserInterfaceTool
    {
        [Description("Change the background color of the chat interface to the specified color.")]
        public static Task ChangeBackgroundColor(string color)
        {
            Home.Color = color;
            return Task.CompletedTask;
        }
    }
}

