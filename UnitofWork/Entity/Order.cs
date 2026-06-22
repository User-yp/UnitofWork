using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UnitofWork.Entity;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    // EF Core 可通过此构造函数创建实例（支持序列化）
    protected Order()
    {
    }

    public Order(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

internal class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(nameof(Order));
        builder.HasKey(e => e.Id);
    }
}
