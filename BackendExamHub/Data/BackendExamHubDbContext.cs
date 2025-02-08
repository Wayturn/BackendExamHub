using BackendExamHub.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendExamHub.Data
{
    public class BackendExamHubDbContext : DbContext
    {
        public BackendExamHubDbContext(DbContextOptions<BackendExamHubDbContext> options) : base(options) { }

        public DbSet<MyOfficeACPD> MyOfficeACPD { get; set; } = default!;
        public DbSet<MyOfficeExecutionLog> MyOfficeExecutionLog { get; set; } = default!;
    }
}
