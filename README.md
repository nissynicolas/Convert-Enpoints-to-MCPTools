# OpenAPI to MCP Server Generator

🚀 **Convert any OpenAPI JSON specification into a fully functional C# MCP (Model Context Protocol) server!**

This tool automatically generates complete C# MCP server applications where each OpenAPI endpoint becomes an MCP tool, making REST APIs instantly accessible to AI agents and MCP-compatible clients.

## ✨ Features

- 🔄 **Automatic Conversion**: Transform OpenAPI 3.0+ specs into MCP servers
- 🛠️ **Complete Project Generation**: Creates ready-to-run C# projects
- 📡 **MCP Compatible**: Generated servers work with Cursor IDE, VS Code, Claude Desktop
- 🎯 **Smart Naming**: Intelligent tool naming from endpoints and operation IDs
- 🔧 **Type Mapping**: Converts OpenAPI types to appropriate C# types
- 📚 **Documentation**: Auto-generated README and usage examples
- 🧪 **Testing**: Includes test scripts for validation

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK** or later
- OpenAPI 3.0+ JSON specification file

### Installation & Usage

```bash
# Clone or download this generator
git clone <repository-url>
cd OpenApiToMcpGenerator

# Build the generator
dotnet build

# Generate MCP server from OpenAPI spec
dotnet run -- --openapi petstore.json --output ./Generated-PetStore-Server --name PetStoreMcpServer

# Run the generated server
cd Generated-PetStore-Server
dotnet run
```

## 📋 Command Line Options

```bash
dotnet run -- [options]

Options:
  --openapi, -o     Path to OpenAPI JSON file or URL (required)
                    Examples: 
                    • ./petstore.json
                    • https://petstore3.swagger.io/api/v3/openapi.json
                    • https://localhost:56733/swagger/v1/swagger.json
  --output, -out    Output directory for generated MCP server (required)
  --name, -n        Name for the generated project (default: GeneratedMcpServer)
  --base-url, -b    Base URL for the API (overrides OpenAPI server URLs)
  --namespace, -ns  Root namespace for generated code (default: GeneratedMcpServer)
  --verbose, -v     Enable verbose output
  --help           Show help information
```

## 🎯 Example Usage

### Generate from Swagger Petstore (URL)
```bash
# Generate directly from URL - no need to download first!
dotnet run -- --openapi https://petstore3.swagger.io/api/v3/openapi.json --output ./PetStore-MCP --name PetStoreMcpServer --base-url https://petstore3.swagger.io/api/v3

# Test the generated server
cd PetStore-MCP
dotnet run
```

### Generate from Local Development Server
```bash
# Generate from your local API's swagger endpoint
dotnet run -- --openapi https://localhost:56733/swagger/v1/swagger.json --output ./MyApi-MCP --name MyApiMcpServer --base-url https://localhost:56733

# Or from a local file
dotnet run -- --openapi ./my-api-spec.json --output ./MyApi-MCP --name MyApiMcpServer
```

### Integration with Cursor IDE
```json
{
  "mcpServers": {
    "petstore": {
      "command": "dotnet",
      "args": ["run", "--project", "./PetStore-MCP", "--no-build"],
      "cwd": "./PetStore-MCP"
    }
  }
}
```

## 🏗️ Generated Project Structure

```
Generated-MCP-Server/
├── 📄 Program.cs              # MCP server entry point (identical to reference)
├── 📄 ProjectName.csproj      # Project file with MCP dependencies
├── 📄 ApiTools.cs             # Generated MCP tools from OpenAPI endpoints
├── 📄 README.md               # Generated documentation
└── 📄 test-mcp.bat           # Testing script
```

## 🔄 Conversion Examples

### OpenAPI Endpoint → MCP Tool

**OpenAPI:**
```json
{
  "paths": {
    "/users/{id}": {
      "get": {
        "operationId": "getUserById",
        "summary": "Get user by ID",
        "parameters": [
          {"name": "id", "in": "path", "required": true, "schema": {"type": "string"}}
        ]
      }
    }
  }
}
```

**Generated MCP Tool:**
```csharp
[McpServerTool, Description("Get user by ID")]
public static async Task<string> GetUserById(
    [Description("The user ID to retrieve")] string id)
{
    try
    {
        using var client = new HttpClient();
        var url = $"{BaseUrl}/users/{id}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            data = JsonSerializer.Deserialize<object>(responseContent),
            method = "GET",
            url = url
        }, new JsonSerializerOptions { WriteIndented = true });
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new
        {
            success = false,
            error = ex.Message,
            method = "GET",
            endpoint = "/users/{id}"
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
```

## 🎯 Supported Features

### ✅ HTTP Methods
- GET, POST, PUT, PATCH, DELETE
- Custom HTTP methods

### ✅ Parameters
- Path parameters (`/users/{id}`)
- Query parameters (`?limit=10&offset=0`)
- Request bodies (JSON, form-data, etc.)
- Header parameters

### ✅ OpenAPI Features
- Operation IDs for tool naming
- Parameter descriptions
- Request/response schemas
- Multiple content types
- Server URLs

### ✅ Generated Code Features
- Async/await patterns
- Proper error handling
- JSON serialization
- Type-safe parameter handling
- Comprehensive documentation

## 🔧 Architecture

The generator consists of several key components:

- **OpenApiParser**: Parses OpenAPI specs and extracts endpoint information
- **CodeGenerator**: Generates C# MCP tool methods from endpoints
- **ProjectGenerator**: Creates complete project structure
- **NameGenerator**: Smart naming for tools and parameters
- **TypeMapper**: Maps OpenAPI types to C# types

## 🧪 Testing Generated Servers

### Manual Testing
```bash
# Test tools list
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' | dotnet run

# Test specific tool
echo '{"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {"name": "GetUsers", "arguments": {}}}' | dotnet run
```

### Using Test Script
```bash
# Windows
test-mcp.bat

# The script tests basic functionality and tool calls
```

## 🎯 Compatible MCP Clients

Generated servers work with any MCP client supporting JSON-RPC over stdin/stdout:

- ✅ **Cursor IDE** (Native MCP support)
- ✅ **VS Code** (With MCP extensions)
- ✅ **Claude Desktop** (With configuration)
- ✅ **Custom MCP clients**

## 📚 Examples

See the `examples/` directory for sample OpenAPI specifications and their generated MCP servers:

- **Petstore API** → Pet management tools
- **JSONPlaceholder** → Blog post and user management
- **GitHub API** → Repository and issue management

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## 📄 License

This project is available under the **MIT License**.

## 🔗 References

- [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Official MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)

---

**Transform your REST APIs into AI-accessible tools with the power of MCP! 🚀**
