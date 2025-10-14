using Humanizer;
using System.Text.RegularExpressions;

namespace OpenApiToMcpGenerator.Utils;

/// <summary>
/// Generates appropriate names for MCP tools based on OpenAPI operations
/// </summary>
public static class NameGenerator
{
    /// <summary>
    /// Generate a tool name from HTTP method, path, and operation ID
    /// </summary>
    /// <param name="httpMethod">HTTP method (GET, POST, etc.)</param>
    /// <param name="path">API path (/users/{id})</param>
    /// <param name="operationId">OpenAPI operation ID (if available)</param>
    /// <returns>Generated tool name</returns>
    public static string GenerateToolName(string httpMethod, string path, string? operationId)
    {
        // Use operation ID if available and valid
        if (!string.IsNullOrWhiteSpace(operationId) && IsValidIdentifier(operationId))
        {
            return ToPascalCase(operationId);
        }

        // Generate from HTTP method and path
        return GenerateFromPath(httpMethod, path);
    }

    /// <summary>
    /// Generate tool name from HTTP method and path
    /// </summary>
    private static string GenerateFromPath(string httpMethod, string path)
    {
        // Remove leading slash and split by slash
        var pathParts = path.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        // Remove parameter placeholders and clean up parts
        var cleanParts = pathParts
            .Where(part => !part.StartsWith('{'))
            .Select(part => part.Replace("-", "").Replace("_", ""))
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        // Build the name
        var baseName = string.Join("", cleanParts.Select(ToPascalCase));
        
        // Add HTTP method prefix
        var methodPrefix = httpMethod.ToLower() switch
        {
            "get" => "Get",
            "post" => "Create",
            "put" => "Update",
            "patch" => "Update",
            "delete" => "Delete",
            _ => ToPascalCase(httpMethod)
        };

        // Handle special cases
        if (string.IsNullOrEmpty(baseName))
        {
            baseName = "Resource";
        }

        // Check if path has ID parameter to make it singular/plural appropriate
        var hasIdParam = pathParts.Any(part => part.Contains("{id}") || part.Contains("{") && part.Contains("id"));
        
        if (hasIdParam && methodPrefix == "Get")
        {
            baseName = baseName.Singularize();
            return $"{methodPrefix}{baseName}ById";
        }
        
        if (methodPrefix == "Get" && !hasIdParam)
        {
            baseName = baseName.Pluralize();
        }
        else if (methodPrefix != "Get")
        {
            baseName = baseName.Singularize();
        }

        return $"{methodPrefix}{baseName}";
    }

    /// <summary>
    /// Convert string to PascalCase
    /// </summary>
    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Handle camelCase and PascalCase
        if (Regex.IsMatch(input, @"^[a-zA-Z][a-zA-Z0-9]*$"))
        {
            return char.ToUpper(input[0]) + input[1..];
        }

        // Handle snake_case and kebab-case
        var words = Regex.Split(input, @"[_\-\s]+")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => char.ToUpper(w[0]) + w[1..].ToLower());

        return string.Join("", words);
    }

    /// <summary>
    /// Check if a string is a valid C# identifier
    /// </summary>
    private static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        return Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }
}
