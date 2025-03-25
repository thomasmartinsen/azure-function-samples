using Agents;
using Agents.Extensions;

public class AgentService(IAgentHandler agentHandler) : IAgentService
{
    public async Task RunAsync()
    {
        _ = await agentHandler.InitializeAgentAsync();
        _ = await agentHandler.InitializeAgentThreadAsync();

        var userMessage = "I need to solve the equation `3x + 11 = 14`. Can you help me?";
        var additionalInstructions = "Address the user as Hansen. The user has a premium account.";
        var runResponse = await agentHandler.CreateAndRunUserMessageAsync(userMessage, additionalInstructions);

        var result = await agentHandler.ProcessResponseAsync(runResponse);
        result?.PrintMessages(false);
    }
}
