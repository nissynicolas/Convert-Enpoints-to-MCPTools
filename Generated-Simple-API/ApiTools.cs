using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace GeneratedMcpServer;

[McpServerToolType]
public static class ApiTools
{
    private const string BaseUrl = "https://api.example.com/v1";

    [McpServerTool, Description("Get all users")]
    public static async Task<string> GetUsers([Description("Maximum number of users to return")] string? limit = null)
    {
        try
        {
            using var client = new HttpClient();

            var url = $"{BaseUrl}/users";

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(limit))
                queryParams.Add($"limit={limit}");
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

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
                endpoint = "/users"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new user")]
    public static async Task<string> CreateUser([Description("Request body data in application/json format")] string requestBody)
    {
        try
        {
            using var client = new HttpClient();

            var url = $"{BaseUrl}/users";

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Serialize(new
            {
                success = true,
                data = JsonSerializer.Deserialize<object>(responseContent),
                method = "POST",
                url = url
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                method = "POST",
                endpoint = "/users"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get user by ID")]
    public static async Task<string> GetUserById([Description("The user ID")] string id)
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

}
