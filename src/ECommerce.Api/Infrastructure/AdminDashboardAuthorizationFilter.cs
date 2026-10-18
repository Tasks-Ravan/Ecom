using ECommerce.Api.Domain;
using Hangfire.Dashboard;

namespace ECommerce.Api.Infrastructure;

public sealed class AdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true && user.IsInRole(UserRole.Admin);
    }
}
