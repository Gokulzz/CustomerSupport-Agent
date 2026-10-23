using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerSupport.Application.Results;

namespace CustomerSupport.Infrastructure.Agents;

internal sealed record ToolResult(bool Success, object? Data, string? ErrorCode, string? Message)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static ToolResult Ok(object data) => new(true, data, null, null);

    public static ToolResult Fail(string code, string message) => new(false, null, code, message);

    public static ToolResult Fail(Error error) => Fail(error.Code, error.Description);

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);
}
