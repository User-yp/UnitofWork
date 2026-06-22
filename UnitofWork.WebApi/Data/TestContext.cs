using Microsoft.EntityFrameworkCore;
using UnitofWork.WebApi.Data.Entities;

namespace UnitofWork.WebApi.Data;

public class TestContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public TestContext(DbContextOptions<TestContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
