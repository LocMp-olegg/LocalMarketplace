namespace LocMp.Gateway.Api.Options;

public class CorsPoliticsOptions
{
    public string AllowedOrigins { get; set; } = string.Empty;

    public string[] GetOrigins() =>
        AllowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
