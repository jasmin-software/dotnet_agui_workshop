# Creating an AG-UI Client

The AG-UI client connects to the remote server and displays streaming responses.

From inside the _AGUI_ folder, run these terminal window commands to create a client console application

```bash
dotnet new console -n ConsoleAGUI
dotnet sln add ./ConsoleAGUI/ConsoleAGUI.csproj
```

**Required Packages**

Install the necessary packages for the client:

```bash
cd ConsoleAGUI
dotnet add package Microsoft.Agents.AI.AGUI --prerelease
dotnet add package Microsoft.Agents.AI --prerelease
cd ..
```

The Microsoft.Agents.AI package provides the AsAIAgent() extension method.

Replace the contents of _/ConsoleAGUI/Program.cs_ with the following code:

```C#
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

string serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:8888";

Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}\n");

// Create the AG-UI client agent
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

AGUIChatClient chatClient = new(httpClient, serverUrl);

AIAgent agent = chatClient.AsAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent");

AgentSession session = await agent.GetNewSessionAsync();
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
        bool isFirstUpdate = true;
        string? threadId = null;

        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(messages, session))
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
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[Run Finished - Thread: {threadId}]");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nAn error occurred: {ex.Message}");
}
```

**Testing the client**

To test the client, you must run the server first. Therefore, open a terminal window in the _ServerAGUI_ folder and run the following command:

```bash
dotnet run --urls http://localhost:8888
```

You can now run the client console application in the _ConsoleAGUI_ folder with this command:

```bash
dotnet run
```

When prompted fot input, I entered the following:

```
Tell me about settlements on the moon.
```

This is the output I received:

```
[Run Started - Thread: thread_2dd2fade9de14cac9c8ac7d4cb04b51e, Run: run_2d96143b5eec46098a79e8e89fa92335]
Settlements on the Moon, often referred to as lunar colonies or bases, are a subject of great interest in space exploration and technology. Here are some key points about the concept of Moon settlements:

### 1. **Purpose of Lunar Settlements**
   - **Scientific Research**: They would provide a platform for scientific research, including geology, astronomy, and even biology in a low-gravity environment.
   - **Resource Exploration**: The Moon is rich in resources like Helium-3, rare earth metals, and water ice, which could be used for fuel and life support.
   - **Gateway to Mars and Beyond**: A Moon base could serve as a staging ground for missions to Mars and further into the solar system.

### 2. **Technological Requirements**
   - **Habitat Construction**: Building durable habitats that can withstand radiation, extreme temperatures, and micrometeorite impacts is essential.
   - **Life Support Systems**: Closed-loop systems for air, water, and food production will be crucial, possibly involving hydroponics and advanced recycling technologies.
   - **Power Generation**: Solar panels are likely candidates for power generation, given the Moon's 14 Earth-day long days.

### 3. **International Efforts and Collaboration**
   - Various nations and organizations are planning or exploring the possibility of lunar settlements, including NASA's Artemis program, which aims to land “the first woman and the next man” on the Moon and establish a sustainable human presence by the end of the 2020s.
   - The European Space Agency (ESA), China's CNSA, and private companies like SpaceX and Blue Origin are also actively engaged in lunar exploration.

### 4. **Challenges**
   - **Health Risks**: Prolonged exposure to low gravity and radiation poses health hazards for astronauts and settlers.
   - **Logistical Challenges**: Transporting materials and people to the Moon will be costly and complex.
   - **Legal and Ethical Considerations**: The legal framework for ownership, exploitation of resources, and responsibilities for damage or pollution remains under discussion in the context of international space law.

### 5. **Vision for the Future**
   - Some visions include permanent habitats with infrastructure for research and even tourism.
   - Advances in AI, robotics, and construction technology might allow for the initial stages of settlement to be automated.

In conclusion, lunar settlements represent a fascinating frontier in human exploration, blending science, technology, and collaboration. While there are significant challenges to overcome, the potential benefits for humanity are vast and varied.
[Run Finished - Thread: thread_2dd2fade9de14cac9c8ac7d4cb04b51e]
```
