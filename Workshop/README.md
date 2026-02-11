# Workshop

## [0. GitHub Token](0.%20GitHub%20Token/README.md)

**Create a GitHub Personal Access Token (PAT).**

We will use an AI model hosted on GitHub. Your AG-UI server needs a PAT to authenticate with GitHub and make requests to the model API.

### Goal
By the end of this step, you will have:
- A GitHub PAT created
- The PAT saved locally for use by your server

## [1. Server Setup](./1.%20Server%20Setup/README.md)

**Build an AG-UI server to host your AI agent.**

The server is where the agent's logic runs and where it is exposed via HTTP endpoints.

### Goal
By the end of this step, you will have:
- An AG-UI server running
- An AI agent hosted on the server
- An HTTP endpoint that streams agent responses

## [2. Client Setup](2.%20Client%20Setup/README.md)

**Create a user interface that connects to the server and displays streaming responses from your agent.**

The client is where users interact with the agent and see results in real time.

### Goal
By the end of this step, you will have:
- A client connected to the AG-UI server
- User input sent to the agent
- Streaming responses displayed in the UI

## [3. Tools](./3.%20Tools/README.md) 

**Add function tools your agent can call to perform specific tasks.**

Tools allow agents to do more than generate text — they can act on systems and UIs.

### Goal
By the end of this step, you will have:
- A backend tool that runs on the server
- A frontend tool that runs on the client
- An agent that knows when to call each tool

## [4. Human-in-the-Loop](./4.%20Human-in-the-Loop/README.md)

**Add function tools that require human approval before your agent executes certain tasks.**

Human-in-the-loop keeps humans in control of high-impact or sensitive actions.

### Goal
By the end of this step, you will have:
- A tool that requires user approval
- An agent that pauses while waiting for input
- The ability to approve or reject the action and resume execution
