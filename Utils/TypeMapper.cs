using Microsoft.OpenApi.Models;

namespace OpenApiToMcpGenerator.Utils;

/// <summary>
/// Maps OpenAPI types to C# types
/// </summary>
public static class TypeMapper
{
    /// <summary>
    /// Map OpenAPI schema type to C# type
    /// </summary>
    /// <param name="schema">OpenAPI schema</param>
    /// <returns>C# type name</returns>
    public static string MapOpenApiType(OpenApiSchema? schema)
    {
        if (schema == null)
            return "object";

        return schema.Type?.ToLower() switch
        {
            "string" => MapStringType(schema),
            "integer" => MapIntegerType(schema),
            "number" => MapNumberType(schema),
            "boolean" => "bool",
            "array" => MapArrayType(schema),
            "object" => "object",
            _ => "object"
        };
    }

    /// <summary>
    /// Map string types with format considerations
    /// </summary>
    private static string MapStringType(OpenApiSchema schema)
    {
        return schema.Format?.ToLower() switch
        {
            "date" => "DateTime",
            "date-time" => "DateTime",
            "uuid" => "Guid",
            "email" => "string",
            "uri" => "string",
            "byte" => "byte[]",
            "binary" => "byte[]",
            _ => "string"
        };
    }

    /// <summary>
    /// Map integer types with format considerations
    /// </summary>
    private static string MapIntegerType(OpenApiSchema schema)
    {
        return schema.Format?.ToLower() switch
        {
            "int32" => "int",
            "int64" => "long",
            _ => "int"
        };
    }

    /// <summary>
    /// Map number types with format considerations
    /// </summary>
    private static string MapNumberType(OpenApiSchema schema)
    {
        return schema.Format?.ToLower() switch
        {
            "float" => "float",
            "double" => "double",
            "decimal" => "decimal",
            _ => "double"
        };
    }

    /// <summary>
    /// Map array types
    /// </summary>
    private static string MapArrayType(OpenApiSchema schema)
    {
        var itemType = MapOpenApiType(schema.Items);
        return $"{itemType}[]";
    }

    /// <summary>
    /// Get the parameter type for MCP tool parameters (always string for simplicity)
    /// </summary>
    /// <param name="schema">OpenAPI schema</param>
    /// <returns>Parameter type for MCP tools</returns>
    public static string GetMcpParameterType(OpenApiSchema? schema)
    {
        // For MCP tools, we'll use string parameters and convert as needed
        // This simplifies the interface while maintaining flexibility
        return "string";
    }

    /// <summary>
    /// Get conversion code from string to the target type
    /// </summary>
    /// <param name="schema">OpenAPI schema</param>
    /// <param name="parameterName">Name of the parameter to convert</param>
    /// <returns>C# code to convert the parameter</returns>
    public static string GetConversionCode(OpenApiSchema? schema, string parameterName)
    {
        if (schema == null)
            return parameterName;

        var targetType = MapOpenApiType(schema);

        return targetType switch
        {
            "string" => parameterName,
            "int" => $"int.Parse({parameterName})",
            "long" => $"long.Parse({parameterName})",
            "float" => $"float.Parse({parameterName})",
            "double" => $"double.Parse({parameterName})",
            "decimal" => $"decimal.Parse({parameterName})",
            "bool" => $"bool.Parse({parameterName})",
            "DateTime" => $"DateTime.Parse({parameterName})",
            "Guid" => $"Guid.Parse({parameterName})",
            _ => parameterName
        };
    }
}
