using System.ComponentModel;
using Client.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

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

        [Description("Generate a text file with the specified filename and content, and return the URL to the generated file.")]
        public static string GenerateTextFile(
            [Description("The filename to generate")] string filename,
            [Description("The content to write to the file")] string content)
        {
            string webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "output");
            if (!Directory.Exists(webRoot))
                Directory.CreateDirectory(webRoot);
            string filePath = Path.Combine(webRoot, filename);

            File.WriteAllText(filePath, content);

            return $"/output/{filename}";
        }
    }
}

