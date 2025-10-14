using OpenApiToMcpGenerator.Models;

namespace OpenApiToMcpGenerator.Core;

/// <summary>
/// Generates C# code for MCP tools from API endpoints
/// </summary>
public class CodeGenerator
{
    /// <summary>
    /// Generate the main tools class containing all MCP tools
    /// </summary>
    /// <param name="endpoints">List of API endpoints</param>
    /// <param name="namespaceName">Namespace for the generated code</param>
    /// <param name="baseUrl">Base URL for the API</param>
    /// <returns>Generated C# code</returns>
    public string GenerateToolsClass(List<ApiEndpoint> endpoints, string namespaceName, string? baseUrl = null)
    {
        var code = new System.Text.StringBuilder();

        // Add usings
        code.AppendLine("using System.ComponentModel;");
        code.AppendLine("using System.Text;");
        code.AppendLine("using System.Text.Json;");
        code.AppendLine("using ModelContextProtocol.Server;");
        code.AppendLine();

        // Add namespace
        code.AppendLine($"namespace {namespaceName};");
        code.AppendLine();

        // Add tools class
        code.AppendLine("[McpServerToolType]");
        code.AppendLine("public static class ApiTools");
        code.AppendLine("{");

        // Add base URL constant if provided
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            code.AppendLine($"    private const string BaseUrl = \"{baseUrl}\";");
            code.AppendLine();
        }

        // Generate tools for each endpoint
        foreach (var endpoint in endpoints)
        {
            GenerateToolMethod(code, endpoint, !string.IsNullOrWhiteSpace(baseUrl));
            code.AppendLine();
        }

        code.AppendLine("}");

        return code.ToString();
    }

    /// <summary>
    /// Generate a single MCP tool method for an endpoint
    /// </summary>
    private void GenerateToolMethod(System.Text.StringBuilder code, ApiEndpoint endpoint, bool hasBaseUrl)
    {
        // Add tool attribute and description
        code.AppendLine($"    [McpServerTool, Description(\"{EscapeString(endpoint.Description)}\")]");
        
        // Method signature
        code.Append($"    public static async Task<string> {endpoint.ToolName}(");
        
        // Add parameters (required first, then optional)
        var requiredParameters = new List<string>();
        var optionalParameters = new List<string>();
        
        // Add path parameters (always required)
        foreach (var param in endpoint.PathParameters)
        {
            requiredParameters.Add($"[Description(\"{EscapeString(param.Description)}\")] string {param.Name}");
        }
        
        // Add request body parameter for POST/PUT/PATCH (always required if present)
        if (endpoint.RequestBody != null)
        {
            var bodyParamName = endpoint.RequestBody.ContentType.Contains("json") ? "requestBody" : "requestData";
            var bodyDescription = !string.IsNullOrEmpty(endpoint.RequestBody.Description) 
                ? endpoint.RequestBody.Description 
                : $"Request body data in {endpoint.RequestBody.ContentType} format";
            requiredParameters.Add($"[Description(\"{EscapeString(bodyDescription)}\")] string {bodyParamName}");
        }
        
        // Add query parameters (can be optional)
        foreach (var param in endpoint.QueryParameters)
        {
            var paramDef = param.IsRequired 
                ? $"[Description(\"{EscapeString(param.Description)}\")] string {param.Name}"
                : $"[Description(\"{EscapeString(param.Description)}\")] string? {param.Name} = null";
            
            if (param.IsRequired)
                requiredParameters.Add(paramDef);
            else
                optionalParameters.Add(paramDef);
        }
        
        // Combine required and optional parameters
        var parameters = new List<string>();
        parameters.AddRange(requiredParameters);
        parameters.AddRange(optionalParameters);

        code.AppendLine(string.Join(", ", parameters) + ")");
        code.AppendLine("    {");
        code.AppendLine("        try");
        code.AppendLine("        {");
        code.AppendLine("            using var client = new HttpClient();");
        code.AppendLine();

        // Build URL
        GenerateUrlBuilding(code, endpoint, hasBaseUrl);
        code.AppendLine();

        // Generate HTTP request
        GenerateHttpRequest(code, endpoint);

        // Handle response
        code.AppendLine("            response.EnsureSuccessStatusCode();");
        code.AppendLine("            var responseContent = await response.Content.ReadAsStringAsync();");
        code.AppendLine();
        code.AppendLine("            return JsonSerializer.Serialize(new");
        code.AppendLine("            {");
        code.AppendLine("                success = true,");
        code.AppendLine("                data = JsonSerializer.Deserialize<object>(responseContent),");
        code.AppendLine($"                method = \"{endpoint.HttpMethod}\",");
        code.AppendLine("                url = url");
        code.AppendLine("            }, new JsonSerializerOptions { WriteIndented = true });");
        code.AppendLine("        }");
        code.AppendLine("        catch (Exception ex)");
        code.AppendLine("        {");
        code.AppendLine("            return JsonSerializer.Serialize(new");
        code.AppendLine("            {");
        code.AppendLine("                success = false,");
        code.AppendLine("                error = ex.Message,");
        code.AppendLine($"                method = \"{endpoint.HttpMethod}\",");
        code.AppendLine($"                endpoint = \"{endpoint.Path}\"");
        code.AppendLine("            }, new JsonSerializerOptions { WriteIndented = true });");
        code.AppendLine("        }");
        code.AppendLine("    }");
    }

    /// <summary>
    /// Generate URL building code
    /// </summary>
    private void GenerateUrlBuilding(System.Text.StringBuilder code, ApiEndpoint endpoint, bool hasBaseUrl)
    {
        var urlPath = endpoint.Path;
        
        // Replace path parameters
        foreach (var param in endpoint.PathParameters)
        {
            urlPath = urlPath.Replace($"{{{param.Name}}}", $"{{{param.Name}}}");
        }

        if (hasBaseUrl)
        {
            code.AppendLine($"            var url = $\"{{BaseUrl}}{urlPath}\";");
        }
        else
        {
            code.AppendLine($"            // TODO: Configure your API base URL");
            code.AppendLine($"            var baseUrl = \"https://api.example.com\"; // Replace with your API base URL");
            code.AppendLine($"            var url = $\"{{baseUrl}}{urlPath}\";");
        }

        // Add query parameters if any
        if (endpoint.QueryParameters.Any())
        {
            code.AppendLine();
            code.AppendLine("            var queryParams = new List<string>();");
            
            foreach (var param in endpoint.QueryParameters)
            {
                if (param.IsRequired)
                {
                    code.AppendLine($"            queryParams.Add($\"{param.Name}={{{param.Name}}}\");");
                }
                else
                {
                    code.AppendLine($"            if (!string.IsNullOrEmpty({param.Name}))");
                    code.AppendLine($"                queryParams.Add($\"{param.Name}={{{param.Name}}}\");");
                }
            }
            
            code.AppendLine("            if (queryParams.Any())");
            code.AppendLine("                url += \"?\" + string.Join(\"&\", queryParams);");
        }
    }

    /// <summary>
    /// Generate HTTP request code
    /// </summary>
    private void GenerateHttpRequest(System.Text.StringBuilder code, ApiEndpoint endpoint)
    {
        switch (endpoint.HttpMethod.ToUpper())
        {
            case "GET":
                code.AppendLine("            var response = await client.GetAsync(url);");
                break;
            
            case "DELETE":
                code.AppendLine("            var response = await client.DeleteAsync(url);");
                break;
            
            case "POST":
            case "PUT":
            case "PATCH":
                if (endpoint.RequestBody != null)
                {
                    var bodyParamName = endpoint.RequestBody.ContentType.Contains("json") ? "requestBody" : "requestData";
                    code.AppendLine($"            var content = new StringContent({bodyParamName}, Encoding.UTF8, \"{endpoint.RequestBody.ContentType}\");");
                    code.AppendLine($"            var response = await client.{endpoint.HttpMethod.ToLower().Pascalize()}Async(url, content);");
                }
                else
                {
                    code.AppendLine($"            var response = await client.{endpoint.HttpMethod.ToLower().Pascalize()}Async(url, null);");
                }
                break;
            
            default:
                code.AppendLine($"            var request = new HttpRequestMessage(HttpMethod.{endpoint.HttpMethod.Pascalize()}, url);");
                code.AppendLine("            var response = await client.SendAsync(request);");
                break;
        }
    }

    /// <summary>
    /// Escape string for C# string literals
    /// </summary>
    private string EscapeString(string input)
    {
        return input?.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") ?? string.Empty;
    }
}

/// <summary>
/// Extension methods for string manipulation
/// </summary>
public static class StringExtensions
{
    public static string Pascalize(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return char.ToUpper(input[0]) + input[1..].ToLower();
    }
}
