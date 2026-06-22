using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnitofWork.DbContexts;
using UnitofWork.Domain;
using UnitofWork.InUnitOfWork;
using UnitofWork.IRepository;
using UnitofWork.Repository;
using UnitofWork.UnitOfWork;

namespace UnitofWork;

public static class ServiceExtension
{
    public static IServiceCollection InitService(this IServiceCollection service, IConfiguration configuration)
    {
        var conn = configuration.GetSection(nameof(ConnectionOption)).Get<ConnectionOption>()
            ?? throw new InvalidOperationException(
                $"Missing required configuration section '{nameof(ConnectionOption)}'. " +
                "Please ensure appsettings.json contains ConnectionOption with TestConnStr and QuartzConnStr.");

        if (string.IsNullOrWhiteSpace(conn.TestConnStr))
            throw new InvalidOperationException("ConnectionOption:TestConnStr is required but was empty.");
        if (string.IsNullOrWhiteSpace(conn.QuartzConnStr))
            throw new InvalidOperationException("ConnectionOption:QuartzConnStr is required but was empty.");

        // 使用 AddDbContext 注册为 Scoped，与 UnitOfWork 生命周期一致
        service.AddDbContext<TestContext>(opt =>
        {
            opt.UseMySql(
                conn.TestConnStr,
                ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        service.AddDbContext<QuartzContext>(opt =>
        {
            opt.UseMySql(
                conn.QuartzConnStr,
                ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        service.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        service.AddScoped<IContextCenter, ContextCenter>();
        service.AddScoped<IRepoFactory, RepoFactory>();
        service.AddScoped<IOrderRepository, OrderRepository>();
        service.AddScoped<IQuartzRepository, QuartzRepository>();
        service.AddScoped<DomainService>();
        return service;
    }
}
