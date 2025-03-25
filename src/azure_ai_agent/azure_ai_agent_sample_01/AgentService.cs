using Agents;
using Microsoft.Extensions.Configuration;

public class AgentService(IConfigurationRoot config, IAgentHandler agentHandler) : IAgentService
{
    public async Task RunAsync()
    {
        Azure.AI.Projects.Agent agent;

        try
        {
            agent = await agentHandler.InitializeAgentAsync();
            Console.WriteLine($"found agent with name {agent.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error: {ex.Message}");
            return;
        }
    }
}
