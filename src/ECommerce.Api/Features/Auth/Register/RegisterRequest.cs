namespace ECommerce.Api.Features.Auth.Register;

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public sealed record RegisterResponse(Guid UserId, string FirstName, string LastName, string Email);
