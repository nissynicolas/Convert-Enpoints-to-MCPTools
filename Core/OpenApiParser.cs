using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using OpenApiToMcpGenerator.Models;
using OpenApiToMcpGenerator.Utils;

namespace OpenApiToMcpGenerator.Core;

/// <summary>
/// Parses OpenAPI specifications and extracts endpoint information
/// </summary>
public class OpenApiParser
{
    /// <summary>
    /// Parse an OpenAPI specification from file or URL and extract all endpoints
    /// </summary>
    /// <param name="input">Path to OpenAPI JSON file or URL</param>
    /// <param name="verbose">Enable verbose output</param>
    /// <returns>List of parsed API endpoints</returns>
    public async Task<List<ApiEndpoint>> ParseAsync(string input, bool verbose = false)
    {
        string jsonContent;
        
        // Check if input is a URL
        if (Uri.TryCreate(input, UriKind.Absolute, out var uri) && 
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            if (verbose)
            {
                Console.WriteLine($"🌐 Fetching OpenAPI specification from URL: {input}");
            }
            
            jsonContent = await FetchFromUrlAsync(input);
        }
        else
        {
            // Treat as file path
            if (!File.Exists(input))
            {
                throw new FileNotFoundException($"OpenAPI file not found: {input}");
            }

            if (verbose)
            {
                Console.WriteLine($"📖 Reading OpenAPI specification from file: {input}");
            }
            
            jsonContent = await File.ReadAllTextAsync(input);
        }

        // Parse the OpenAPI document
        var reader = new OpenApiStringReader();
        var document = reader.Read(jsonContent, out var diagnostic);

        if (diagnostic.Errors.Any())
        {
            var errors = string.Join("\n", diagnostic.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"OpenAPI parsing errors:\n{errors}");
        }

        if (verbose)
        {
            Console.WriteLine($"📋 OpenAPI Info: {document.Info.Title} v{document.Info.Version}");
            Console.WriteLine($"🔗 Servers: {string.Join(", ", document.Servers.Select(s => s.Url))}");
        }

        var endpoints = new List<ApiEndpoint>();

        // Extract endpoints from paths
        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var endpoint = ExtractEndpoint(path.Key, operation.Key.ToString().ToUpper(), operation.Value);
                endpoints.Add(endpoint);

                if (verbose)
                {
                    Console.WriteLine($"  ➤ {endpoint.HttpMethod} {endpoint.Path} → {endpoint.ToolName}");
                }
            }
        }

        return endpoints;
    }

    /// <summary>
    /// Fetch OpenAPI specification from a URL
    /// </summary>
    /// <param name="url">URL to fetch the OpenAPI specification from</param>
    /// <returns>JSON content as string</returns>
    private async Task<string> FetchFromUrlAsync(string url)
    {
        using var httpClient = new HttpClient();
        
        // Set a reasonable timeout
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Add headers to indicate we want JSON
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenApiToMcpGenerator/1.0");

        try
        {
            var response = await httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to fetch OpenAPI specification from {url}. " +
                    $"Status: {response.StatusCode} ({response.ReasonPhrase})");
            }

            var content = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException($"Empty response received from {url}");
            }

            return content;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"Timeout while fetching OpenAPI specification from {url}");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions as-is
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error fetching OpenAPI specification from {url}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract endpoint information from OpenAPI path and operation
    /// </summary>
    private ApiEndpoint ExtractEndpoint(string path, string httpMethod, OpenApiOperation operation)
    {
        var endpoint = new ApiEndpoint
        {
            Path = path,
            HttpMethod = httpMethod,
            Operation = operation,
            ToolName = NameGenerator.GenerateToolName(httpMethod, path, operation.OperationId),
            Description = operation.Summary ?? operation.Description ?? $"{httpMethod} {path}"
        };

        // Extract parameters
        foreach (var parameter in operation.Parameters ?? new List<OpenApiParameter>())
        {
            var apiParam = new ApiParameter
            {
                Name = parameter.Name,
                Type = TypeMapper.MapOpenApiType(parameter.Schema),
                IsRequired = parameter.Required,
                Description = parameter.Description ?? string.Empty,
                Location = parameter.In?.ToString().ToLower() ?? "query"
            };

            switch (parameter.In)
            {
                case ParameterLocation.Path:
                    endpoint.PathParameters.Add(apiParam);
                    break;
                case ParameterLocation.Query:
                    endpoint.QueryParameters.Add(apiParam);
                    break;
                case ParameterLocation.Header:
                    endpoint.HeaderParameters.Add(apiParam);
                    break;
            }
        }

        // Extract request body
        if (operation.RequestBody != null)
        {
            var contentType = operation.RequestBody.Content.Keys.FirstOrDefault() ?? "application/json";
            var mediaType = operation.RequestBody.Content.Values.FirstOrDefault();

            endpoint.RequestBody = new ApiRequestBody
            {
                ContentType = contentType,
                IsRequired = operation.RequestBody.Required,
                Description = operation.RequestBody.Description ?? string.Empty,
                SchemaType = mediaType?.Schema != null ? TypeMapper.MapOpenApiType(mediaType.Schema) : "object"
            };
        }

        // Extract response information
        var successResponse = operation.Responses.FirstOrDefault(r => 
            r.Key.StartsWith("2") || r.Key == "default").Value;

        if (successResponse != null)
        {
            var responseContentType = successResponse.Content?.Keys.FirstOrDefault();
            var responseMediaType = successResponse.Content?.Values.FirstOrDefault();

            endpoint.Response = new ApiResponse
            {
                StatusCode = "200",
                Description = successResponse.Description ?? string.Empty,
                ContentType = responseContentType,
                SchemaType = responseMediaType?.Schema != null ? TypeMapper.MapOpenApiType(responseMediaType.Schema) : "object"
            };
        }

        return endpoint;
    }
}
