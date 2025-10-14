using Microsoft.OpenApi.Models;

namespace OpenApiToMcpGenerator.Models;

/// <summary>
/// Represents a parsed OpenAPI endpoint with all necessary information for code generation
/// </summary>
public class ApiEndpoint
{
    /// <summary>
    /// The HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public required string HttpMethod { get; set; }

    /// <summary>
    /// The API path (e.g., "/users/{id}")
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// The OpenAPI operation object
    /// </summary>
    public required OpenApiOperation Operation { get; set; }

    /// <summary>
    /// Generated tool name for the MCP server
    /// </summary>
    public required string ToolName { get; set; }

    /// <summary>
    /// Description for the MCP tool
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Path parameters extracted from the endpoint
    /// </summary>
    public List<ApiParameter> PathParameters { get; set; } = new();

    /// <summary>
    /// Query parameters extracted from the endpoint
    /// </summary>
    public List<ApiParameter> QueryParameters { get; set; } = new();

    /// <summary>
    /// Header parameters extracted from the endpoint
    /// </summary>
    public List<ApiParameter> HeaderParameters { get; set; } = new();

    /// <summary>
    /// Request body information (for POST, PUT, PATCH)
    /// </summary>
    public ApiRequestBody? RequestBody { get; set; }

    /// <summary>
    /// Expected response information
    /// </summary>
    public ApiResponse? Response { get; set; }
}

/// <summary>
/// Represents a parameter in an API endpoint
/// </summary>
public class ApiParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Parameter type (string, int, bool, etc.)
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Whether the parameter is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Parameter description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Default value if any
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Parameter location (path, query, header)
    /// </summary>
    public required string Location { get; set; }
}

/// <summary>
/// Represents request body information
/// </summary>
public class ApiRequestBody
{
    /// <summary>
    /// Content type (application/json, etc.)
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// Whether the request body is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Description of the request body
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Schema information for the request body
    /// </summary>
    public string? SchemaType { get; set; }
}

/// <summary>
/// Represents response information
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public string StatusCode { get; set; } = "200";

    /// <summary>
    /// Response description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Content type of the response
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Schema information for the response
    /// </summary>
    public string? SchemaType { get; set; }
}
