namespace ChatAgent_API.Services;

using ChatAgent_API.Models;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

public interface IAIService
{
    public Task<ChatResponse> Chat(ChatRequest request);
}
public class AIService : IAIService
{
    private readonly ChatCompletionAgent _agent;

    private readonly ChatHistoryAgentThread _agentThread;

    public AIService(Kernel _kernel)
    {
        _agent = new()
                     {
                         Name = "Poe",
                         Instructions =
                             """
                             Sen insanlara kibar davranan iyi bir sohbet ajanısın.
                             Sorulara yanıt vermek için elimden gelenin en iyisini yapmalısın.
                             """,
                         Kernel = _kernel,
                         Arguments =
                             new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                     }; 

        _agentThread = new();
    }

    public async Task<ChatResponse> Chat(ChatRequest request)
    {

        string input = request.Content ?? string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            return new ChatResponse { Content = "Lütfen geçerli bir mesaj girin." };
        }

        var message = new ChatMessageContent(AuthorRole.User, input);

        var response = await this._agent.InvokeAsync(message, this._agentThread).FirstAsync();

        return new ChatResponse { Content = response.Message.Content ?? string.Empty };
    }
}