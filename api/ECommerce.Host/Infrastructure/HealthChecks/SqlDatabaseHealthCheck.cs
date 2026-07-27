using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce.Host.Infrastructure.HealthChecks;

/// <summary>Every module shares the same physical database, so one connectivity check covers all of them.</summary>
public sealed class SqlDatabaseHealthCheck(string? connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return HealthCheckResult.Unhealthy("No database connection string configured.");

        try
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString) { ConnectTimeout = 3 };
            await using SqlConnection connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unreachable.", ex);
        }
    }
}
