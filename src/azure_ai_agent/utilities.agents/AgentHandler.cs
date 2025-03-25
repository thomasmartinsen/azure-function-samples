using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Agents;

public class AgentHandler : IAgentHandler
{
    private readonly IConfigurationRoot _config;
    private AgentsClient _agentsClient;
    private AIProjectClient _projectClient;
    private Agent? _agent;
    private AgentThread? _thread;
    private const int RESPONSE_DELAY_IN_MILLISECONDS = 500;

    public AgentsClient AgentsClient => _agentsClient ?? throw new Exception("not initialized");

    public AIProjectClient AIProjectClient => _projectClient ?? throw new Exception("not initialized");

    public AgentThread Thread => _thread ?? throw new Exception("not initialized");

    public Agent Agent => _agent ?? throw new Exception("not initialized");

    public AgentHandler(IConfigurationRoot config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _agentsClient = InitializeAgentsClient();
    }

    public AgentsClient InitializeAgentsClient()
    {
        var connectionString = _config["AI_PROJECT_CONNECTIONSTRING"] ?? throw new Exception("missing project connectionstring");
        _ = connectionString ?? throw new Exception(nameof(connectionString));

        var client = new AgentsClient(connectionString, new DefaultAzureCredential());
        _ = client ?? throw new Exception("client not initialized");

        _agentsClient = client;

        return client;
    }

    public AIProjectClient InitializeProjectClient(
        AIProjectClientOptions? clientOptions = null)
    {
        var connectionString = _config["AI_PROJECT_CONNECTIONSTRING"] ?? throw new Exception("missing project connectionstring");
        _ = connectionString ?? throw new Exception(nameof(connectionString));

        clientOptions ??= new AIProjectClientOptions();

        var client = new AIProjectClient(connectionString, new DefaultAzureCredential(), clientOptions);
        _ = client ?? throw new Exception("client not initialized");

        _projectClient = client;
        _agentsClient = client.GetAgentsClient();

        return client;
    }

    public async Task<Agent> InitializeAgentAsync(
        string? agentName = null,
        bool createAgentIfNotExists = true,
        bool setAgentThread = false,
        IEnumerable<ToolDefinition>? tools = null,
        ToolResources? toolResources = null)
    {
        agentName ??= _config["AI_AGENT_NAME"] ?? throw new Exception("missing agent name");

        Response<PageableList<Agent>> agentListResponse =
             await _agentsClient.GetAgentsAsync();

        Agent? agent = agentListResponse.Value.FirstOrDefault(x => x.Name == agentName);

        if (agent == null && createAgentIfNotExists == true)
        {
            var agentInstructions = _config["AI_AGENT_INSTRUCTIONS"] ?? throw new Exception("missing agent instructions");
            var agentModelName = _config["AI_MODEL_NAME"] ?? throw new Exception("missing model name");

            tools ??= [new CodeInterpreterToolDefinition()];
            toolResources ??= new ToolResources();

            Response<Agent> agentResponse =
                await _agentsClient.CreateAgentAsync(
                    model: agentModelName,
                    name: agentName,
                    instructions: agentInstructions,
                    tools: tools,
                    toolResources: toolResources);
            agent = agentResponse.Value;
        }

        _agent = agent ?? throw new Exception($"no agent with name {agentName} could be found");

#if DEBUG
        Console.WriteLine($"hello, i'm your agent {_agent.Name}");
#endif

        if (setAgentThread == true)
        {
            _thread = await InitializeAgentThreadAsync();
        }

        return _agent;
    }

    public async Task<AgentThread> InitializeAgentThreadAsync(
        bool force = false)
    {
        if (_thread == null || force == true)
        {
            Response<AgentThread> threadResponse = await _agentsClient.CreateThreadAsync();
            _thread = threadResponse.Value;
        }

        return _thread;
    }

    public async Task CreateUserMessageAsync(
        string userMessage)
    {
        _ = userMessage ?? throw new ArgumentNullException(nameof(userMessage));
        _ = _thread ?? throw new Exception("thread not initialized");

        await _agentsClient.CreateMessageAsync(_thread.Id, MessageRole.User, userMessage);
    }

    public async Task<Response<ThreadRun>> CreateAndRunUserMessageAsync(
        string userMessage,
        string? additionalInstructions)
    {
        _ = userMessage ?? throw new ArgumentNullException(nameof(userMessage));
        _ = _thread ?? throw new Exception("thread not initialized");
        _ = _agent ?? throw new Exception("agent not initialized");

        await CreateUserMessageAsync(userMessage);

        return await _agentsClient.CreateRunAsync(_thread.Id, _agent.Id, additionalInstructions: additionalInstructions);
    }

    public async Task<Response<PageableList<ThreadMessage>>> ProcessResponseAsync(
        Response<ThreadRun> runResponse)
    {
        _ = _thread ?? throw new Exception("thread not initialized");

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(RESPONSE_DELAY_IN_MILLISECONDS));
            runResponse = await _agentsClient.GetRunAsync(_thread.Id, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

        return await _agentsClient.GetMessagesAsync(_thread.Id);
    }
}
