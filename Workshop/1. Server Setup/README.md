# Creating an AG-UI Server

From a working directory inside of a terminal window, create a web app named _ServerAGUI_ with the following command:

```bash
mkdir AGUI
cd AGUI
dotnet new sln
dotnet new web -n ServerAGUI
dotnet sln add ./ServerAGUI/ServerAGUI.csproj
cd ServerAGUI
mkdir Tools
cd ..
dotnet new gitignore
```

The AG-UI server hosts your AI agent and exposes it via HTTP endpoints using ASP.NET Core.

**Required Packages**

Install the necessary packages for the server:

```bash 
cd ServerAGUI
dotnet add package Azure.AI.OpenAI --prerelease
dotnet add package Azure.Identity
dotnet add package Microsoft.Agents.AI.Hosting.AGUI.AspNetCore --version 1.0.0-preview.260108.1
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.260108.1
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
cd ..
```

**Server Code**

Add the following JSON to _appsettings.Development.json_ file:

```json
"GitHub": {
    "Token": "put-your-github-personal-access-token-here",
    "ApiEndpoint": "https://models.github.ai/inference",
    "Model": "openai/gpt-4o-mini"
}
```

*NOTE*: Replace _put-your-github-personal-access-token-here_ with your GitHub Personal Access Token. 

Edit the _.gitignore_ file in the _AGUI_ folder and add to it _appsettings.Development.json_ so that your secrets do not find their way into source control by mistake.

We will add a simple weather tool that returns a static weather report. Of course, you can replace the tool with one that connects to a weather API and returns real weather related forecast data. 

In the _ServerAGUI/Tools_ folder, add a C# class named _WeatherBackendTool.cs_ with this code:

```C#
using System.ComponentModel;

namespace ServerAGUI.Tools;

internal static class WeatherBackendTool
{
    [Description("Lookup the weather in a location.")]
    public static string GetWeather(string location)
    {
        // In a real implementation, this would call a weather API.
        return $"The weather in {location} is sunny with a high of 75°F.";
    }
}
```

In _Program.cs_, add this code right before statement ``var app = builder.Build();``:

```C#
builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();
```

To read environment variables: add this code below statement ``var app = builder.Build();``:

```C#
string? apiKey = builder.Configuration["GitHub:Token"];
string? endpoint = builder.Configuration["GitHub:ApiEndpoint"] ?? "https://models.github.ai/inference";
string? deploymentName = builder.Configuration["GitHub:Model"] ?? "openai/gpt-4o-mini";
```

Right below the above code, add the following code that registers our weather tool and creates an agent named _Socrates_ who is a motivational speaker:

```C#
// Create AI agent
AITool[] tools =
[
    AIFunctionFactory.Create(WeatherBackendTool.GetWeather)
];

// Create AI agent
var agent = new OpenAIClient(
    new ApiKeyCredential(apiKey!),
    new OpenAIClientOptions()
    {
        Endpoint = new Uri(endpoint)
    })
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "You're a wise motivational speaker.",
        name: "Socrates",
        tools: tools);
```

Finally, replace the endpoint statement ``app.MapGet("/", () => "Hello World!");`` with this AG-UI endpoint:

```C#
// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);
```

This is it, folks. We now have an AG-UI server that we will later connect to from a variety of clients.
