using Microsoft.EntityFrameworkCore;
using UnitofWork.Entity;

namespace UnitofWork.DbContexts;

public class QuartzContext : DbContext
{
    public DbSet<JobConfig> JobConfigs { get; set; }

    public QuartzContext(DbContextOptions<QuartzContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
