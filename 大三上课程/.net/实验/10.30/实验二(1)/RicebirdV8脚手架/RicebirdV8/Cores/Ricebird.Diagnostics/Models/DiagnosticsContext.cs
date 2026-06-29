using Microsoft.EntityFrameworkCore;

namespace Ricebird.Diagnostics.Models
{
    public class DiagnosticsContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ConnectLog> ConnectLogs => Set<ConnectLog>();

        public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();

        public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

        public DbSet<UserLog> UserLogs => Set<UserLog>();
    }
}
