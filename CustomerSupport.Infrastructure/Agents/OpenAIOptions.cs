namespace CustomerSupport.Infrastructure.Agents;

public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string Model { get; set; } = string.Empty;


    public string ApiKey { get; set; } = string.Empty;
}
