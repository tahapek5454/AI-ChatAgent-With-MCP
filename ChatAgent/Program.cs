using ChatAgent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ModelContextProtocol.Client;


string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

IConfiguration config = configurationBuilder.Build();

string modelId = config["AI:ModelId"] ?? "";
string apiKey = config["AI:ApiKey"] ?? "";
string apiEndpoint = config["AI:ApiEndpoint"] ?? ""; // OpenRouter uzerinden kullanılacaksa AddOpenAIChatCompletion konfigurasyonunda kullanılacak
string[] mcpServers = config["MCPServers"]?.Split(';') ?? Array.Empty<string>();


List<McpClientTool> toolResponse = new List<McpClientTool>();
int serverCount = 0;
foreach (var server in mcpServers)
{
    serverCount++;
    AIMCPClient aIMCP = new($"MCP-server-{serverCount}", server);
    toolResponse.AddRange(await aIMCP.GetToolListAsync());
}

Console.WriteLine("Kernel Oluşturuluyor...");
IKernelBuilder builder = Kernel.CreateBuilder();


//builder.AddAzureOpenAIChatCompletion(apiKey: "api-key", deploymentName: "your-deployment-name", endpoint: "endpoint");

builder
    .AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: apiKey
    );


if(toolResponse.Any())
{
    builder.Plugins.AddFromFunctions("UserTool", toolResponse.Select(_tool => _tool.AsKernelFunction()));
}


Kernel kernel = builder.Build();

Console.WriteLine("Agent oluşturuluyor...");
ChatCompletionAgent agent =
    new()
    {
        Name = "SampleAssistantAgent",
        Instructions =
                """
                        Senin adın {{$name}}

                        Sen insanlara kibar davranan iyi bir sohbet ajanısın.
                        Sorulara yanıt vermek için elimden gelenin en iyisini yapmalısın.

                        Mevcut tarih ve saat bilgisi: {{$now}}. 
                        """,
        Kernel = kernel,
        Arguments =
            new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            {
                        { "$name", "Poe" }
            }
    };

Console.WriteLine("Hazır! Sohbeti bitirmek için 'EXIT' yazınız.");


ChatHistoryAgentThread agentThread = new();
bool isComplete = false;


do
{
    Console.WriteLine();
    Console.Write("> ");
    string input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }
    if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }

    var message = new ChatMessageContent(AuthorRole.User, input);

    Console.WriteLine();

    DateTime now = DateTime.Now;
    KernelArguments arguments =
        new()
        {
                    { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
        };
    await foreach (var chatResponse in agent.InvokeStreamingAsync(message, agentThread, options: new() { KernelArguments = arguments }))
    {
        Console.Write($"{chatResponse.Message}");
    }

    Console.WriteLine();

} while (!isComplete);

Console.ReadLine();