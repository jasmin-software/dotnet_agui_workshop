# Build your own AI Agent using Microsoft Agent Framework & AG-UI

## Looking for something specific? 

- [AG-UI Blazor app](https://github.com/jasmin-software/dotnet_agui_blazor_demo)
- [Workshop slides](./Slides.pdf)

## Overview

We will be building a **console app** with a client-server architecture.

The AG-UI server hosts your AI agent and a backend tool that fetches weather. The client lets you interact with the agent, change console text color, and approve actions like creating a text file. 

> [!TIP] 
> **Want a sneak peek?**
> 
> The `Demo` folder contains the completed version of the console app. 
> 
> Feel free to explore it first, or use it as a guide as we build everything from scratch together!

## [0. GitHub Token](0.%20GitHub%20Token/README.md)

**Create a GitHub Personal Access Token (PAT).**

We will use an AI model hosted on GitHub. Your AG-UI server needs a PAT to authenticate with GitHub and make requests to the model API.

### Goal
By the end of this step, you will have:
- A GitHub PAT created
- The PAT saved locally

## [1. Server Setup](./1.%20Server%20Setup/README.md)

**Build an AG-UI server to host your AI agent.**

The server runs the agent's logic and exposes it via HTTP endpoints.

### Goal
By the end of this step, you will have:
- A running AG-UI server
- An AI agent hosted on the server
- An HTTP endpoint listening for requests

## [2. Client Setup](2.%20Client%20Setup/README.md)

**Create a user interface that connects to the server and displays streaming responses from your agent.**

The client is where users interact with the agent and see results in real time.

### Goal
By the end of this step, you will have:
- A client connected to the AG-UI server
- User input sent to the agent
- Agent responses streamed and displayed in the UI

## [3. Tools](./3.%20Tools/README.md) 

**Add function tools your agent can call to perform specific tasks.**

Tools allow agents to do more than generate text — they can interact with external systems and modify UIs.

### Goal
By the end of this step, you will have:
- A backend tool that return the weather for a location
- A frontend tool that changes the console text color
- An agent that decides when to call each tool

## [4. Human-in-the-Loop](./4.%20Human-in-the-Loop/README.md)

**Add function tools that require human approval before your agent can execute them.**

Human-in-the-loop keeps humans in control of high-impact or sensitive actions.

### Goal
By the end of this step, you will have:
- A tool that requests user approval before creating a text file
- An agent that pauses while waiting for input
- The ability to approve or reject action and resume execution
