using OpenApiToMcpGenerator.Models;

namespace OpenApiToMcpGenerator.Core;

/// <summary>
/// Generates the complete MCP server project structure
/// </summary>
public class ProjectGenerator
{
    private readonly CodeGenerator _codeGenerator;

    public ProjectGenerator()
    {
        _codeGenerator = new CodeGenerator();
    }

    /// <summary>
    /// Generate the complete MCP server project
    /// </summary>
    /// <param name="options">Generation options</param>
    /// <param name="endpoints">Parsed API endpoints</param>
    public async Task GenerateProjectAsync(GeneratorOptions options, List<ApiEndpoint> endpoints)
    {
        // Create output directory
        if (Directory.Exists(options.OutputDirectory))
        {
            if (options.Verbose)
            {
                Console.WriteLine($"🗂️  Output directory already exists: {options.OutputDirectory}");
            }
        }
        else
        {
            Directory.CreateDirectory(options.OutputDirectory);
            if (options.Verbose)
            {
                Console.WriteLine($"📁 Created output directory: {options.OutputDirectory}");
            }
        }

        // Generate project file
        await GenerateProjectFileAsync(options);

        // Generate Program.cs
        await GenerateProgramFileAsync(options);

        // Generate API tools
        await GenerateApiToolsAsync(options, endpoints);

        // Generate README
        await GenerateReadmeAsync(options, endpoints);

        // Generate test script
        await GenerateTestScriptAsync(options);

        // Generate management scripts
        await GenerateManagementScriptsAsync(options);

        if (options.Verbose)
        {
            Console.WriteLine($"✅ Generated {endpoints.Count} MCP tools in {options.OutputDirectory}");
        }
    }

    /// <summary>
    /// Generate the .csproj file
    /// </summary>
    private async Task GenerateProjectFileAsync(GeneratorOptions options)
    {
        var projectContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>{options.ProjectName}</AssemblyName>
    <RootNamespace>{options.RootNamespace}</RootNamespace>
    <!-- Prevent file locking issues during development -->
    <UseAppHost>false</UseAppHost>
    <SelfContained>false</SelfContained>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.Hosting"" Version=""9.0.9"" />
    <PackageReference Include=""ModelContextProtocol"" Version=""0.4.0-preview.1"" />
  </ItemGroup>

</Project>";

        var filePath = Path.Combine(options.OutputDirectory, $"{options.ProjectName}.csproj");
        await File.WriteAllTextAsync(filePath, projectContent);

        if (options.Verbose)
        {
            Console.WriteLine($"📄 Generated project file: {filePath}");
        }
    }

    /// <summary>
    /// Generate Program.cs (identical to reference server)
    /// </summary>
    private async Task GenerateProgramFileAsync(GeneratorOptions options)
    {
        var programContent = @"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(null);

// Add MCP server following the reference implementation
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();";

        var filePath = Path.Combine(options.OutputDirectory, "Program.cs");
        await File.WriteAllTextAsync(filePath, programContent);

        if (options.Verbose)
        {
            Console.WriteLine($"📄 Generated Program.cs: {filePath}");
        }
    }

    /// <summary>
    /// Generate the API tools class
    /// </summary>
    private async Task GenerateApiToolsAsync(GeneratorOptions options, List<ApiEndpoint> endpoints)
    {
        var toolsCode = _codeGenerator.GenerateToolsClass(endpoints, options.RootNamespace, options.BaseUrl);
        
        var filePath = Path.Combine(options.OutputDirectory, "ApiTools.cs");
        await File.WriteAllTextAsync(filePath, toolsCode);

        if (options.Verbose)
        {
            Console.WriteLine($"📄 Generated API tools: {filePath}");
        }
    }

    /// <summary>
    /// Generate README.md
    /// </summary>
    private async Task GenerateReadmeAsync(GeneratorOptions options, List<ApiEndpoint> endpoints)
    {
        var readmeContent = $@"# {options.ProjectName}

A Model Context Protocol (MCP) server generated from OpenAPI specification.

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK** or later
- **VS Code** with MCP extensions or **Cursor IDE** (recommended)

### Build and Run

```bash
# Safe build (recommended for development)
safe-build.bat

# Or manually:
dotnet build
dotnet run
```

### Development Scripts

- **`safe-build.bat`**: Safely stops any running server instances and rebuilds
- **`stop-server.bat`**: Stops all running server instances
- **`test-mcp.bat`**: Tests the MCP server functionality

The server communicates via JSON-RPC over stdin/stdout, making it compatible with any MCP client.

## 🛠️ Integration with IDEs

### Cursor IDE (Recommended)
Add to your Cursor MCP configuration:

```json
{{
  ""mcpServers"": {{
    ""{options.ProjectName.ToLower()}"": {{
      ""command"": ""dotnet"",
      ""args"": [""run"", ""--project"", ""path/to/{options.ProjectName}"", ""--no-build""],
      ""cwd"": ""path/to/{options.ProjectName}""
    }}
  }}
}}
```

## 📚 Available Tools

This MCP server provides **{endpoints.Count} tools** generated from the OpenAPI specification:

{string.Join("\n", endpoints.Select(e => $"- **{e.ToolName}**: {e.Description}"))}

## 📋 Usage Examples

### List All Available Tools
```json
{{""jsonrpc"": ""2.0"", ""id"": 1, ""method"": ""tools/list""}}
```

### Example Tool Call
```json
{{
  ""jsonrpc"": ""2.0"", ""id"": 2, ""method"": ""tools/call"",
  ""params"": {{
    ""name"": ""{endpoints.FirstOrDefault()?.ToolName ?? "ExampleTool"}"",
    ""arguments"": {{}}
  }}
}}
```

## ⚙️ Configuration

{(string.IsNullOrWhiteSpace(options.BaseUrl) ? 
@"**Important**: Update the base URL in `ApiTools.cs` to point to your actual API endpoint.

```csharp
// In ApiTools.cs, replace:
var baseUrl = ""https://api.example.com""; // Replace with your API base URL
```" : 
$@"**Base URL**: This server is configured to use `{options.BaseUrl}` as the API base URL.")}

## 🔧 Technical Details

- **Framework**: .NET 9.0
- **MCP SDK**: ModelContextProtocol v0.4.0-preview.1
- **Transport**: JSON-RPC over stdin/stdout
- **Generated Tools**: {endpoints.Count}

## 📄 License

This generated project follows the same license as the source OpenAPI specification.

---

**Generated by OpenAPI to MCP Server Generator**";

        var filePath = Path.Combine(options.OutputDirectory, "README.md");
        await File.WriteAllTextAsync(filePath, readmeContent);

        if (options.Verbose)
        {
            Console.WriteLine($"📄 Generated README: {filePath}");
        }
    }

    /// <summary>
    /// Generate test script
    /// </summary>
    private async Task GenerateTestScriptAsync(GeneratorOptions options)
    {
        var testScriptContent = $@"@echo off
echo Testing {options.ProjectName} MCP Server
echo.

echo Testing tools/list...
echo {{""jsonrpc"": ""2.0"", ""id"": 1, ""method"": ""tools/list""}} | dotnet run

echo.
echo Testing tools/call (first tool)...
echo {{""jsonrpc"": ""2.0"", ""id"": 2, ""method"": ""tools/call"", ""params"": {{""name"": ""GetUsers"", ""arguments"": {{}}}}}} | dotnet run

echo.
echo Test completed.
pause";

        var filePath = Path.Combine(options.OutputDirectory, "test-mcp.bat");
        await File.WriteAllTextAsync(filePath, testScriptContent);

        if (options.Verbose)
        {
            Console.WriteLine($"📄 Generated test script: {filePath}");
        }
    }

    /// <summary>
    /// Generate management scripts for safe server operations
    /// </summary>
    private async Task GenerateManagementScriptsAsync(GeneratorOptions options)
    {
        // Generate stop script
        var stopScriptContent = $@"@echo off
echo Stopping {options.ProjectName} MCP Server...

REM Kill any running instances of the server
taskkill /F /IM ""{options.ProjectName}.exe"" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Server stopped successfully.
) else (
    echo No running server instances found.
)

REM Clean up any locked files
if exist ""bin\Debug\net9.0\{options.ProjectName}.exe"" (
    echo Cleaning up executable...
    timeout /t 2 /nobreak >nul
)

echo Ready for rebuild.
pause";

        var stopScriptPath = Path.Combine(options.OutputDirectory, "stop-server.bat");
        await File.WriteAllTextAsync(stopScriptPath, stopScriptContent);

        // Generate safe build script
        var buildScriptContent = $@"@echo off
echo Safe Build and Run for {options.ProjectName}
echo.

REM Stop any running instances first
call stop-server.bat

echo.
echo Building project...
dotnet build --configuration Debug

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build successful! You can now run the server with: dotnet run
echo Or use your MCP client configuration.
pause";

        var buildScriptPath = Path.Combine(options.OutputDirectory, "safe-build.bat");
        await File.WriteAllTextAsync(buildScriptPath, buildScriptContent);

        if (options.Verbose)
        {
            Console.WriteLine($"📄 Generated management scripts: {stopScriptPath}, {buildScriptPath}");
        }
    }
}
