using OpenApiToMcpGenerator.Models;

namespace OpenApiToMcpGenerator.Core;

/// <summary>
/// Main generator class that orchestrates the conversion from OpenAPI to MCP server
/// </summary>
public class McpServerGenerator
{
    private readonly OpenApiParser _parser;
    private readonly CodeGenerator _codeGenerator;
    private readonly ProjectGenerator _projectGenerator;

    public McpServerGenerator()
    {
        _parser = new OpenApiParser();
        _codeGenerator = new CodeGenerator();
        _projectGenerator = new ProjectGenerator();
    }

    /// <summary>
    /// Generate a complete MCP server project from an OpenAPI specification
    /// </summary>
    /// <param name="options">Generation options</param>
    public async Task GenerateAsync(GeneratorOptions options)
    {
        if (options.Verbose)
        {
            Console.WriteLine($"🔍 Parsing OpenAPI specification: {options.OpenApiFilePath}");
        }

        // Parse the OpenAPI specification
        var endpoints = await _parser.ParseAsync(options.OpenApiFilePath, options.Verbose);

        if (options.Verbose)
        {
            Console.WriteLine($"📊 Found {endpoints.Count} endpoints to convert");
        }

        // Generate the project structure
        await _projectGenerator.GenerateProjectAsync(options, endpoints);

        if (options.Verbose)
        {
            Console.WriteLine("✨ Project generation completed successfully");
        }
    }
}
