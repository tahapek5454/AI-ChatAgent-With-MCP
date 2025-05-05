using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

namespace ChatAgent.Services
{
    public class AIMCPClient(string MCPServerName, string MCPServerPath)
    {
        private IMcpClient _mcpClient;
        public async Task<IMcpClient> GetClientAsync()
        {
            if (_mcpClient == null)
                _mcpClient = await McpClientFactory.CreateAsync(
                         clientTransport: new StdioClientTransport(new()
                         {
                             Name = MCPServerName,
                             Command = MCPServerPath
                         }),
                         clientOptions: new McpClientOptions()
                         {
                             ClientInfo = new Implementation()
                             {
                                 Name = "AI.MCP.Client",
                                 Version = "1.0.0"
                             }
                         }
                     );

            return _mcpClient;
        }

        public async Task<IList<McpClientTool>> GetToolListAsync()
        {
            _mcpClient = await GetClientAsync();
            IList<McpClientTool> tools = await _mcpClient.ListToolsAsync();
            return tools;
        }
    }
}
