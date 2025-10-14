namespace OpenApiToMcpGenerator.Models;

/// <summary>
/// Configuration options for the MCP server generator
/// </summary>
public class GeneratorOptions
{
    /// <summary>
    /// Path to the OpenAPI JSON specification file
    /// </summary>
    public required string OpenApiFilePath { get; set; }

    /// <summary>
    /// Output directory for the generated MCP server project
    /// </summary>
    public required string OutputDirectory { get; set; }

    /// <summary>
    /// Name for the generated MCP server project
    /// </summary>
    public required string ProjectName { get; set; }

    /// <summary>
    /// Base URL for the API (overrides OpenAPI server URLs)
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Root namespace for the generated code
    /// </summary>
    public required string RootNamespace { get; set; }

    /// <summary>
    /// Enable verbose output during generation
    /// </summary>
    public bool Verbose { get; set; }
}
