using ChatAgent_API.Models;
using ChatAgent_API.Plugins;
using ChatAgent_API.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var fileAnalysisService = new FileAnalysisPlugin();
builder.Services
    .AddKernel()
    .AddAzureOpenAIChatCompletion
    (apiKey: "",
        deploymentName: "",
        endpoint: "")
    .Plugins.AddFromObject(fileAnalysisService, "FileAnalysis"); ;



builder.Services.AddSingleton<IAIService, AIService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("chat", async ([FromBody] ChatRequest request, IAIService aiService) => 
{
    var response = await aiService.Chat(request);
    return Results.Ok(response);
});


app.MapControllers();

app.Run();
