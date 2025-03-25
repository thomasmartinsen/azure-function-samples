using Agents;
using Agents.Extensions;
using Azure;
using Azure.AI.Projects;

public class AgentService(IAgentHandler agentHandler) : IAgentService
{
    public async Task RunAsync()
    {
        Response<AgentFile> uploadAgentFileResponse =
            await agentHandler.AgentsClient.UploadFileAsync(
                filePath: "Data/book.pdf",
                purpose: AgentFilePurpose.Agents);

        AgentFile uploadedAgentFile = uploadAgentFileResponse.Value;

        CodeInterpreterToolResource codeInterpreterToolResource = new();
        codeInterpreterToolResource.FileIds.Add(uploadedAgentFile.Id);

        _ = await agentHandler.InitializeAgentAsync(
            tools: [new CodeInterpreterToolDefinition()],
            toolResources: new ToolResources { CodeInterpreter = codeInterpreterToolResource },
            setThread: true);

        var userMessage = "what can you tell me about microservices in .net based on perspectives from Christian Horsdal Gammelgaard?";
        var additionalInstructions = "";
        var runResponse = await agentHandler.CreateAndRunUserMessageAsync(userMessage, additionalInstructions);

        var result = await agentHandler.ProcessResponseAsync(runResponse);
        result?.PrintMessages(false);
    }
}
