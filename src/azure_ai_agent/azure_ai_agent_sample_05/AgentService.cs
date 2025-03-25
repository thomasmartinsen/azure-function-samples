using Agents;
using Agents.Extensions;
using Azure.AI.Projects;

public class AgentService(IAgentHandler agentHandler) : IAgentService
{
    public async Task RunAsync()
    {
        var clientOptions = new AIProjectClientOptions();
        agentHandler.InitializeProjectClient(clientOptions);

        ListConnectionsResponse connections =
            await agentHandler.AIProjectClient
                .GetConnectionsClient()
                .GetConnectionsAsync(ConnectionType.AzureAISearch)
                .ConfigureAwait(false);

        if (connections?.Value == null || connections.Value.Count == 0)
        {
            throw new InvalidOperationException("No connections found for the Azure AI Search.");
        }

        string connectionId = connections.Value[0].Id;

        AzureAISearchResource searchResource = new();
        searchResource.IndexList.Add(new IndexResource(connectionId, "books-index"));

        _ = await agentHandler.InitializeAgentAsync(
            tools: [new AzureAISearchToolDefinition()],
            toolResources: new ToolResources { AzureAISearch = searchResource },
            setThread: true);

        var userMessage = "what can you tell me about microservices in .net based on perspectives from Christian Horsdal Gammelgaard?";
        var additionalInstructions = "";
        var runResponse = await agentHandler.CreateAndRunUserMessageAsync(userMessage, additionalInstructions);

        var result = await agentHandler.ProcessResponseAsync(runResponse);
        result?.PrintMessages(false);
    }
}
