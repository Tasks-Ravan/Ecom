using ECommerce.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in new[] { UserRole.Admin, UserRole.Customer })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole<Guid>(role));
                EnsureSuccess(result, $"Could not create role '{role}'.");
            }
        }

        var adminOptions = services.GetRequiredService<IOptions<AdminOptions>>().Value;
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(adminOptions.Email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(), UserName = adminOptions.Email, Email = adminOptions.Email,
                EmailConfirmed = true, FirstName = adminOptions.FirstName, LastName = adminOptions.LastName
            };
            var result = await users.CreateAsync(admin, adminOptions.Password);
            EnsureSuccess(result, "Could not create the configured administrator.");
        }
        if (!await users.IsInRoleAsync(admin, UserRole.Admin))
        {
            var result = await users.AddToRoleAsync(admin, UserRole.Admin);
            EnsureSuccess(result, "Could not assign the administrator role.");
        }
    }

    private static void EnsureSuccess(IdentityResult result, string message)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"{message} {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }
}
