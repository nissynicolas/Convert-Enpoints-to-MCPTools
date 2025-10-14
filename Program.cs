using System.CommandLine;
using OpenApiToMcpGenerator.Core;
using OpenApiToMcpGenerator.Models;

namespace OpenApiToMcpGenerator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("OpenAPI to MCP Server Generator - Convert OpenAPI specifications to C# MCP servers");

        // Input OpenAPI file or URL option
        var openApiOption = new Option<string>(
            aliases: ["--openapi", "-o"],
            description: "Path to OpenAPI JSON file or URL (e.g., https://api.example.com/swagger.json)")
        {
            IsRequired = true
        };
        openApiOption.AddValidator(result =>
        {
            var input = result.GetValueForOption(openApiOption);
            if (!string.IsNullOrEmpty(input))
            {
                // Check if it's a URL
                if (Uri.TryCreate(input, UriKind.Absolute, out var uri) && 
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    // Valid URL, no further validation needed here
                    return;
                }
                
                // Check if it's a file path
                if (!File.Exists(input))
                {
                    result.ErrorMessage = $"OpenAPI file '{input}' does not exist, and it's not a valid URL.";
                }
            }
        });

        // Output directory option
        var outputOption = new Option<DirectoryInfo>(
            aliases: ["--output", "-out"],
            description: "Output directory for the generated MCP server project")
        {
            IsRequired = true
        };

        // Server name option
        var nameOption = new Option<string>(
            aliases: ["--name", "-n"],
            description: "Name for the generated MCP server project",
            getDefaultValue: () => "GeneratedMcpServer");

        // Base URL option
        var baseUrlOption = new Option<string>(
            aliases: ["--base-url", "-b"],
            description: "Base URL for the API (overrides OpenAPI server URLs)");

        // Namespace option
        var namespaceOption = new Option<string>(
            aliases: ["--namespace", "-ns"],
            description: "Root namespace for the generated code",
            getDefaultValue: () => "GeneratedMcpServer");

        // Verbose option
        var verboseOption = new Option<bool>(
            aliases: ["--verbose", "-v"],
            description: "Enable verbose output");

        rootCommand.AddOption(openApiOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(nameOption);
        rootCommand.AddOption(baseUrlOption);
        rootCommand.AddOption(namespaceOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(async (openApiInput, outputDir, name, baseUrl, namespaceName, verbose) =>
        {
            try
            {
                var options = new GeneratorOptions
                {
                    OpenApiFilePath = openApiInput, // Now supports both file paths and URLs
                    OutputDirectory = outputDir.FullName,
                    ProjectName = name,
                    BaseUrl = baseUrl,
                    RootNamespace = namespaceName,
                    Verbose = verbose
                };

                var generator = new McpServerGenerator();
                await generator.GenerateAsync(options);

                Console.WriteLine($"✅ MCP server generated successfully in: {outputDir.FullName}");
                Console.WriteLine($"📁 Project name: {name}");
                Console.WriteLine($"🚀 To run: cd {outputDir.FullName} && dotnet run");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                Environment.Exit(1);
            }
        }, openApiOption, outputOption, nameOption, baseUrlOption, namespaceOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }
}
