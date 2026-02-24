# AG-UI Console App
**This is the reference implementation of the AG-UI console app you are building in the workshop.**

This project contains a **Server** and a **Client**.

> [!NOTE]
> You'll need a **GitHub Personal Access Token** (PAT) to run this application.
>
> If you don't have one yet, go to [0. GitHub Token](../0.%20GitHub%20Token/README.md) to create it.

## Server

### Configuring the Server

Add the following to `appsettings.Development.json` in the `Server` folder:

```json
"GitHub": {
    "Token": "put-your-github-personal-access-token-here",
    "ApiEndpoint": "https://models.github.ai/inference",
    "Model": "openai/gpt-4o-mini"
}
```

> [!NOTE]
>
> Replace `put-your-github-personal-access-token-here` with your GitHub Personal Access Token. 

### Start the Server

From the `Server` folder:
```
dotnet run --urls http://localhost:5000
```

You should see the server running on `http://localhost:5000`. 

Keep this terminal open.

## Client

### Start the Client

In a new terminal, from the `Client` folder:
```
dotnet run
```

The client will connect to the running server and allow you to interact with the agent.

Now, ask the agent anything!

## What's happening?

See explanation [here](../2.%20Client%20Setup/README.md#whats-happening).
