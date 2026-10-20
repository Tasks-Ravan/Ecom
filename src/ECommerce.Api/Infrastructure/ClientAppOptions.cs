namespace ECommerce.Api.Infrastructure;

public sealed class ClientAppOptions
{
    public const string SectionName = "ClientApp";

    public string BaseUrl { get; set; } = "http://localhost:5173";
}
