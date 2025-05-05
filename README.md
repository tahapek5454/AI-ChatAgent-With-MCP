# ChatAgent with Model Context Protocol (MCP)

A .NET-based chat agent implementation that uses OpenAI's models and Model Context Protocol (MCP) for extensible tool integration. This project demonstrates how to create an AI-powered chat agent with custom tool capabilities.

## Project Structure

The solution consists of two main projects:

1. **ChatAgent**: The main application that implements the chat interface using OpenAI and MCP client.
2. **MCP.Server**: A Model Context Protocol server that provides tools for the chat agent.

## Prerequisites

- .NET 9.0
- Visual Studio 2022 or later
- OpenAI API access

## Configuration

1. Configure your AI settings in `appsettings.json`:

```json
{
    "AI": {
        "ModelId": "YOUR-MODEL",
        "APIKey": "YOUR-KEY",
        "Endpoint": "YOUR-ENDPOINT"
    },
    "MCPServers": "PATH-TO-MCP-SERVER"
}
```

Replace the placeholders with your actual values:
- `ModelId`: Your OpenAI model ID (e.g., "gpt-4", "gpt-3.5-turbo")
- `APIKey`: Your OpenAI API key
- `Endpoint`: Your OpenAI API endpoint (usually "https://api.openai.com")
- `MCPServers`: Path to the MCP.Server executable (semicolon-separated for multiple servers)

## Features

- Interactive chat interface with AI-powered responses
- Integration with Model Context Protocol (MCP) for extensible tools
- Custom tool implementation example (UserTool) that fetches user data
- Support for multiple MCP servers
- Environment-specific configuration support
- Streaming chat responses for better user experience

## Technologies Used

- Microsoft.SemanticKernel for AI integration
- Model Context Protocol (MCP) for tool integration
- OpenAI API for chat completion
- .NET 9.0 features
- Microsoft.Extensions.Configuration for configuration management

## Getting Started

1. Clone the repository
2. Build the solution
3. Configure your `appsettings.json` with appropriate AI credentials
4. Run the MCP.Server project
5. Run the ChatAgent project

## Usage

1. Start the application
2. Type your messages in the console
3. Use 'EXIT' to end the chat session
4. The agent will respond using both its AI capabilities and available MCP tools

## Tool Integration

The project includes a sample tool (`UserTool`) that demonstrates how to integrate external functionality:

```csharp
[McpServerToolType]
public class UserTool
{
    [McpServerTool, Description("Tüm kullanıcıları, bilgileriyle birlikte getirir.")]
    public async Task<List<object>> GetUsersAsync()
    {
        // Implementation
    }
}
```

You can add more tools by creating new classes with the `[McpServerToolType]` attribute and methods with `[McpServerTool]` attribute.

## Development

- Use `appsettings.Development.json` for development-specific settings
- The project supports environment-specific configurations through `DOTNET_ENVIRONMENT` variable
- Tool development should follow the MCP protocol specifications

## Contributing

Feel free to contribute to this project by:
1. Forking the repository
2. Creating a feature branch
3. Submitting a pull request

