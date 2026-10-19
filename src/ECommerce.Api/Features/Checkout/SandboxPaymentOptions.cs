namespace ECommerce.Api.Features.Checkout;

public sealed class SandboxPaymentOptions
{
    public const string SectionName = "Payments:Sandbox";
    public bool Enabled { get; init; }
}
