using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UnitofWork.WebApi.Data.Entities;

public class JobConfig
{
    public string Group { get; set; } = null!;
    public string JobKeyName { get; set; } = null!;
    public string? JobDescription { get; set; }
    public string TriggerKeyName { get; set; } = null!;
    public string? TriggerDescription { get; set; }
    public string Cron { get; set; } = null!;
    public string? CronDescription { get; set; }

    /// <summary>
    /// 是否启用。数据库存储为 "Y"/"N"，应用层使用 bool。
    /// </summary>
    public bool IsEnable { get; set; }

    public JobConfig()
    {
    }

    public JobConfig(string group, string jobKeyName, string? jobDescription, string triggerKeyName,
        string? triggerDescription, string cron, bool isEnabled, string? cronDescription)
    {
        Group = group;
        JobKeyName = jobKeyName;
        JobDescription = jobDescription;
        TriggerKeyName = triggerKeyName;
        TriggerDescription = triggerDescription;
        Cron = cron;
        IsEnable = isEnabled;
        CronDescription = cronDescription;
    }
}

internal class JobConfigConfig : IEntityTypeConfiguration<JobConfig>
{
    public void Configure(EntityTypeBuilder<JobConfig> builder)
    {
        builder.ToTable("JOB_CONFIG");
        builder.HasKey(j => new { j.Group, j.JobKeyName });
        builder.HasIndex(p => p.Group);
        builder.HasIndex(p => p.JobKeyName);

        builder.Property(j => j.Group)
            .HasColumnName("GROUP").IsRequired().HasMaxLength(50);

        builder.Property(j => j.JobKeyName)
            .HasColumnName("JOB_KEYNAME").IsRequired().HasMaxLength(50);

        builder.Property(j => j.JobDescription)
            .HasColumnName("JOB_DESC").HasMaxLength(100);

        builder.Property(j => j.TriggerKeyName)
            .HasColumnName("TRIGGER_KEYNAME").IsRequired().HasMaxLength(50);

        builder.Property(j => j.TriggerDescription)
            .HasColumnName("TRIGGER_DESC").HasMaxLength(100);

        builder.Property(j => j.Cron)
            .HasColumnName("CRON").IsRequired().HasMaxLength(50);

        builder.Property(j => j.CronDescription)
            .HasColumnName("CRON_DESC").HasMaxLength(100);

        builder.Property(j => j.IsEnable)
            .HasColumnName("IS_ENABLE")
            .IsRequired()
            .HasMaxLength(1)
            .HasDefaultValue(false)
            .HasConversion(v => v ? "Y" : "N", v => v == "Y");
    }
}
