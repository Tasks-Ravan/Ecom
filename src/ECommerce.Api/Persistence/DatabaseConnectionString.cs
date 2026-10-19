using Npgsql;

namespace ECommerce.Api.Persistence;

public static class DatabaseConnectionString
{
    public static string Normalize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
            return connectionString;

        var userInfo = Uri.UnescapeDataString(uri.UserInfo).Split(':', 2);
        var database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException("The PostgreSQL connection URL must include a database, username, and password.");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = database,
            Username = userInfo[0],
            Password = userInfo[1]
        };

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (Enum.TryParse<SslMode>(query["sslmode"], true, out var sslMode))
            builder.SslMode = sslMode;

        return builder.ConnectionString;
    }
}
