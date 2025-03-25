using Agents;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

public class AgentService(IConfigurationRoot config) : IAgentService
{
    public async Task RunAsync()
    {
        var connectionString = config["AI_PROJECT_CONNECTIONSTRING"] ?? throw new Exception("missing project connectionstring");
        var agentName = config["AI_AGENT_NAME"] ?? throw new Exception("missing agent name");
        var agentInstructions = config["AI_AGENT_INSTRUCTIONS"] ?? throw new Exception("missing agent instructions");
        var agentModelName = config["AI_MODEL_NAME"] ?? throw new Exception("missing model name");

        var userMessage = "I need to solve the equation `3x + 11 = 14`. Can you help me?";
        var additionalInstructions = "Address the user as Hansen. The user has a premium account.";

        AgentsClient client = new(connectionString, new DefaultAzureCredential());

        Response<PageableList<Agent>> agentListResponse =
            await client.GetAgentsAsync();

        Agent? agent = agentListResponse.Value?.FirstOrDefault(x => x.Name == agentName);

        if (agent == null)
        {
            Response<Agent> agentResponse =
                await client.CreateAgentAsync(
                    model: agentModelName,
                    name: agentName,
                    instructions: agentInstructions,
                    tools: [new CodeInterpreterToolDefinition()]);
            agent = agentResponse.Value;
        }

        Response<AgentThread> threadResponse =
            await client.CreateThreadAsync();
        AgentThread thread = threadResponse.Value;

        Response<ThreadMessage> messageResponse =
            await client.CreateMessageAsync(
                thread.Id,
                MessageRole.User,
                userMessage);
        ThreadMessage message = messageResponse.Value;

        Response<PageableList<ThreadMessage>> messagesListResponse =
            await client.GetMessagesAsync(thread.Id);

        Response<ThreadRun> runResponse =
            await client.CreateRunAsync(
                thread.Id,
                agent.Id,
                additionalInstructions: additionalInstructions);
        ThreadRun run = runResponse.Value;

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

        Response<PageableList<ThreadMessage>> afterRunMessagesResponse =
            await client.GetMessagesAsync(thread.Id);

        PrintMessages(afterRunMessagesResponse.Value.Data, false);
    }

    void PrintMessages(IReadOnlyList<ThreadMessage> messages, bool newestToOldest = true)
    {
        foreach (ThreadMessage threadMessage in (newestToOldest ? messages : messages.Reverse()))
        {
            PrintMessage(threadMessage);
        }
    }

    void PrintMessage(ThreadMessage threadMessage)
    {
        Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");

        foreach (MessageContent contentItem in threadMessage.ContentItems)
        {
            if (contentItem is MessageTextContent textItem)
            {
                Console.Write(textItem.Text);
            }
            else if (contentItem is MessageImageFileContent imageFileItem)
            {
                Console.Write($"<image from ID: {imageFileItem.FileId}");
            }

            Console.WriteLine();
        }
    }
}
