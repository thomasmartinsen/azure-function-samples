using Azure;
using Azure.AI.Projects;

namespace Agents;

public interface IAgentHandler
{
    AgentsClient AgentsClient { get; }

    AIProjectClient AIProjectClient { get; }

    AgentThread Thread { get; }

    Agent Agent { get; }

    AgentsClient InitializeAgentsClient();

    AIProjectClient InitializeProjectClient(
        AIProjectClientOptions? clientOptions = null);

    Task<Agent> InitializeAgentAsync(
        string? agentName = null,
        bool createIfNotExists = true,
        bool setThread = false,
        IEnumerable<ToolDefinition>? tools = null,
        ToolResources? toolResources = null);

    Task<AgentThread> InitializeAgentThreadAsync(
        bool force = false);

    Task CreateUserMessageAsync(
        string userMessage);

    Task<Response<ThreadRun>> CreateAndRunUserMessageAsync(
        string userMessage,
        string? additionalInstructions);

    Task<Response<PageableList<ThreadMessage>>> ProcessResponseAsync(
        Response<ThreadRun> runResponse);
}
