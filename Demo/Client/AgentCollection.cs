using Microsoft.Agents.AI;

namespace Client;

public record AgentCollection(
    ChatClientAgent AssistantAgent,
    ChatClientAgent RepresentativeAgent);